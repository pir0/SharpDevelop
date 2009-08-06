﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Siegfried Pammer" email="sie_pam@gmx.at"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.XmlEditor;

namespace ICSharpCode.XamlBinding
{
	/// <summary>
	/// Description of XamlResolver.
	/// </summary>
	public class XamlResolver : IResolver
	{
		IClass callingClass;
		string resolveExpression;
		int caretLine, caretColumn;
		XamlContext context;

		public ResolveResult Resolve(ExpressionResult expressionResult, ParseInformation parseInfo, string fileContent)
		{
			if (parseInfo == null || string.IsNullOrEmpty(fileContent))
				return null;
			
			this.resolveExpression = expressionResult.Expression;
			this.caretLine = expressionResult.Region.BeginLine;
			this.caretColumn = expressionResult.Region.BeginColumn;
			this.callingClass = parseInfo.BestCompilationUnit.GetInnermostClass(caretLine, caretColumn);
			this.context = expressionResult.Context as XamlContext ?? CompletionDataHelper.ResolveContext(fileContent, parseInfo.MostRecentCompilationUnit.FileName, caretLine, caretColumn);
			
			switch (this.context.Description) {
				case XamlContextDescription.AtTag:
				case XamlContextDescription.None:
					return ResolveElementName(resolveExpression);
				case XamlContextDescription.InTag:
					return ResolveAttribute(resolveExpression) ?? ResolveElementName(resolveExpression);
				case XamlContextDescription.InAttributeValue:
					MemberResolveResult mrr = ResolveAttribute(context.AttributeName.FullXmlName);
					if (mrr != null) {
						var rr = ResolveAttributeValue(mrr.ResolvedMember, resolveExpression) ?? mrr;
						return rr;
					}
					break;
				case XamlContextDescription.InMarkupExtension:
					return ResolveMarkupExtension(resolveExpression);
			}
			
			return null;
		}
		
		ResolveResult ResolveMarkupExtension(string expression)
		{
			if (context.AttributeValue.IsString)
				return null;
			
			object data = Utils.GetMarkupDataAtPosition(context.AttributeValue.ExtensionValue, context.ValueStartOffset);
			
			// resolve markup extension type
			if ((data as string) == expression) {
				return ResolveElementName(expression + "Extension") ?? ResolveElementName(expression);
			} else {
				MarkupExtensionInfo info = Utils.GetMarkupExtensionAtPosition(context.AttributeValue.ExtensionValue, context.ValueStartOffset);
				TypeResolveResult extensionType = (ResolveElementName(info.ExtensionType + "Extension") ?? ResolveElementName(info.ExtensionType)) as TypeResolveResult;
				if (extensionType != null && extensionType.ResolvedType != null) {
					var value = data as AttributeValue;
					
					switch (extensionType.ResolvedType.FullyQualifiedName) {
						case "System.Windows.Markup.StaticExtension":
						case "System.Windows.Markup.TypeExtension":
							if (value != null && value.IsString) {
								return ResolveElementName(expression) ?? ResolveAttribute(expression);
							}
							goto default; // "fall through"
						default:
							if (data is KeyValuePair<string, AttributeValue>) {
								var pair = (KeyValuePair<string, AttributeValue>)data;
								var member = ResolveNamedAttribute(pair.Key);
								if (pair.Value.StartOffset + pair.Key.Length >= context.ValueStartOffset) {
									return member;
								} else {
									if (pair.Value.IsString && member != null)
										return ResolveAttributeValue(member.ResolvedMember, expression) ?? ResolveElementName(expression);
								}
							}
							break;
					}
				}
				
				return null;
			}
		}
		
		MemberResolveResult ResolveNamedAttribute(string expression)
		{
			MarkupExtensionInfo info = Utils.GetMarkupExtensionAtPosition(context.AttributeValue.ExtensionValue, context.ValueStartOffset);
			TypeResolveResult extensionType = (ResolveElementName(info.ExtensionType + "Extension") ?? ResolveElementName(info.ExtensionType)) as TypeResolveResult;
			
			if (extensionType != null && extensionType.ResolvedType != null) {
				return ResolvePropertyName(extensionType.ResolvedType, expression, false);
			}
			
			return null;
		}

		ResolveResult ResolveElementName(string exp)
		{
			string xmlNamespace;
			string name;
			
			this.resolveExpression = exp;
			if (resolveExpression.Contains(":")) {
				string prefix = resolveExpression.Substring(0, resolveExpression.IndexOf(':'));
				name = resolveExpression.Substring(resolveExpression.IndexOf(':') + 1);
				if (!context.XmlnsDefinitions.TryGetValue(prefix, out xmlNamespace))
					xmlNamespace = null;
			}
			else {
				if (!context.XmlnsDefinitions.TryGetValue("", out xmlNamespace))
					xmlNamespace = null;
				name = resolveExpression;
			}
			if (name.Contains(".")) {
				string propertyName = name.Substring(name.IndexOf('.') + 1);
				name = name.Substring(0, name.IndexOf('.'));
				return ResolveProperty(xmlNamespace, name, propertyName, true);
			}
			else {
				IProjectContent pc = context.ParseInformation.BestCompilationUnit.ProjectContent;
				IReturnType resolvedType = XamlCompilationUnit.FindType(pc, xmlNamespace, name);
				IClass resolvedClass = resolvedType != null ? resolvedType.GetUnderlyingClass() : null;
				if (resolvedClass != null) {
					return new TypeResolveResult(callingClass, null, resolvedClass);
				}
				else {
					return null;
				}
			}
		}

		MemberResolveResult ResolveProperty(string xmlNamespace, string className, string propertyName, bool allowAttached)
		{
			IProjectContent pc = context.ParseInformation.BestCompilationUnit.ProjectContent;
			IReturnType resolvedType = XamlCompilationUnit.FindType(pc, xmlNamespace, className);
			if (resolvedType != null && resolvedType.GetUnderlyingClass() != null) {
				var result = ResolvePropertyName(resolvedType, propertyName, allowAttached);
				if (result == null && CompletionDataHelper.XamlNamespaceAttributes.Contains(propertyName))
					return new MemberResolveResult(null, null, new DefaultProperty(null, propertyName));
				return result;
			}
			return null;
		}
		
		MemberResolveResult ResolvePropertyName(IReturnType resolvedType, string propertyName, bool allowAttached)
		{
			IMember member = resolvedType.GetProperties().Find(delegate(IProperty p) { return p.Name == propertyName; });
			if (member == null) {
				member = resolvedType.GetEvents().Find(delegate(IEvent p) { return p.Name == propertyName; });
			}
			if (member == null && allowAttached) {
				IMethod method = resolvedType.GetMethods().Find(
					delegate(IMethod p) {
						return p.IsStatic && p.Parameters.Count == 1 && p.Name == "Get" + propertyName;
					});
				member = method;
				if (member != null) {
					member = new DefaultProperty(resolvedType.GetUnderlyingClass(), propertyName) { ReturnType = method.ReturnType };
				} else {
					IMethod m = resolvedType.GetMethods().Find(
						delegate(IMethod p) {
							return p.IsPublic && p.IsStatic && p.Parameters.Count == 2 && (p.Name == "Add" + propertyName + "Handler" || p.Name == "Remove" + propertyName + "Handler");
						});
					member = m;
					if (member != null)
						member = new DefaultEvent(resolvedType.GetUnderlyingClass(), propertyName) { ReturnType = m.Parameters[1].ReturnType };
				}
			}
			if (member != null)
				return new MemberResolveResult(callingClass, null, member);
			return null;
		}

		MemberResolveResult ResolveAttribute(string attributeName)
		{
			if (context.ActiveElement == null) {
				return null;
			}
			string attributeXmlNamespace;
			if (attributeName.Contains(":")) {
				string prefix = attributeName.Substring(0, attributeName.IndexOf(':'));
				if (!context.XmlnsDefinitions.TryGetValue(prefix, out attributeXmlNamespace))
					attributeXmlNamespace = null;
				attributeName = attributeName.Substring(attributeName.IndexOf(':') + 1);
			}
			else {
				if (!context.XmlnsDefinitions.TryGetValue("", out attributeXmlNamespace))
					attributeXmlNamespace = null;
			}
			if (attributeName.Contains(".")) {
				string className = attributeName.Substring(0, attributeName.IndexOf('.'));
				attributeName = attributeName.Substring(attributeName.IndexOf('.') + 1);
				return ResolveProperty(attributeXmlNamespace, className, attributeName, true);
			}
			else {
				QualifiedNameWithLocation lastElement = context.ActiveElement;
				return ResolveProperty(lastElement.Namespace, lastElement.Name, attributeName, false);
			}
		}

		ResolveResult ResolveAttributeValue(IMember propertyOrEvent, string expression)
		{
			if (propertyOrEvent == null)
				return null;
			if (propertyOrEvent is IEvent && callingClass != null) {
				return new MethodGroupResolveResult(callingClass, null, callingClass.DefaultReturnType, expression);
			} else if (propertyOrEvent is IProperty && callingClass != null) {
				return ResolveElementName(expression);
			}

			if (propertyOrEvent.Name == "Name" && callingClass != null) {
				foreach (IField f in callingClass.Fields) {
					if (f.Name == expression)
						return new MemberResolveResult(callingClass, null, f);
				}
			}

			IReturnType type = propertyOrEvent.ReturnType;
			if (type == null) return null;
			IClass c = type.GetUnderlyingClass();
			if (c == null) return null;

			if (c.ClassType == ClassType.Enum) {
				foreach (IField f in c.Fields) {
					if (f.Name == expression)
						return new MemberResolveResult(callingClass, null, f);
				}
			}
			return null;
		}
		
		public static ResolveResult Resolve(string expression, XamlCompletionContext context)
		{
			return new XamlResolver().Resolve(new ExpressionResult(expression, context), context.ParseInformation, context.Editor.Document.Text);
		}
		
		public static ResolveResult Resolve(string expression, string fileContent, XamlContext context)
		{
			return new XamlResolver().Resolve(new ExpressionResult(expression, context), context.ParseInformation, fileContent);
		}
		
		public static MemberResolveResult ResolveMember(string expression, XamlCompletionContext context)
		{
			if (context.ParseInformation == null || string.IsNullOrEmpty(context.Editor.Document.Text))
				return null;
			
			var expressionResult = new ExpressionResult(expression, context);
			
			XamlResolver resolver = new XamlResolver();
			resolver.resolveExpression = expression;
			resolver.caretLine = expressionResult.Region.BeginLine;
			resolver.caretColumn = expressionResult.Region.BeginColumn;
			resolver.callingClass = context.ParseInformation.BestCompilationUnit.GetInnermostClass(resolver.caretLine, resolver.caretColumn);
			resolver.context = context;
			
			return resolver.ResolveNamedAttribute(expression);
		}
		
		public ArrayList CtrlSpace(int caretLine, int caretColumn, ParseInformation parseInfo, string fileContent, ExpressionContext context)
		{
			return new ArrayList();
		}
	}
}