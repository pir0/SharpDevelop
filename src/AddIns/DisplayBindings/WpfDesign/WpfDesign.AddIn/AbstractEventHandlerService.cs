﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision: 2667$</version>
// </file>

using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;

using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.WpfDesign.Designer.XamlBackend;
using ICSharpCode.Xaml;

namespace ICSharpCode.WpfDesign.AddIn
{
	abstract class AbstractEventHandlerService : IEventHandlerService
	{
		WpfPrimaryViewContent viewContent;

		protected AbstractEventHandlerService(WpfPrimaryViewContent viewContent)
		{
			if (viewContent == null)
				throw new ArgumentNullException("viewContent");
			this.viewContent = viewContent;
		}

		protected IProjectContent GetProjectContent()
		{
			IProject p = ProjectService.OpenSolution.FindProjectContainingFile(viewContent.PrimaryFileName);
			if (p != null)
				return ParserService.GetProjectContent(p) ?? ParserService.DefaultProjectContent;
			else
				return ParserService.DefaultProjectContent;
		}

		protected IClass GetDesignedClass()
		{
			var model = viewContent.Context.ModelService as XamlModelService;
			if (model != null) {
				string className = model.ClassName;
				if (!string.IsNullOrEmpty(className)) {
					return GetProjectContent().GetClass(className, 0);
				}
			}
			return null;
		}

		protected IClass GetDesignedClassCodeBehindPart(IClass c)
		{
			CompoundClass compound = c as CompoundClass;
			if (compound != null) {
				c = null;
				foreach (IClass part in compound.Parts) {
					if (string.IsNullOrEmpty(part.CompilationUnit.FileName))
						continue;
					if (XamlConstants.HasXamlExtension(part.CompilationUnit.FileName))
						continue;
					if (c == null || c.CompilationUnit.FileName.Length > part.CompilationUnit.FileName.Length)
						c = part;
				}
			}
			return c;
		}

		protected abstract void CreateEventHandlerInternal(Type eventHandlerType, string handlerName);

		public void CreateEventHandler(DesignItemProperty eventProperty)
		{
			var item = eventProperty.DesignItem;
			string handlerName = (string)eventProperty.ValueOnInstance;

			if (string.IsNullOrEmpty(handlerName)) {
				if (string.IsNullOrEmpty(item.Name)) {
					GenerateName(eventProperty.DesignItem);
				}
				handlerName = item.Name + "_" + eventProperty.Name;
				eventProperty.SetValue(handlerName);
			}
			CreateEventHandlerInternal(eventProperty.ReturnType, handlerName);
		}

		public DesignItemProperty GetDefaultEvent(DesignItem item)
		{
			object[] attributes = item.ComponentType.GetCustomAttributes(typeof(DefaultEventAttribute), true);
			if (attributes.Length == 1) {
				DefaultEventAttribute dae = (DefaultEventAttribute)attributes[0];
				DesignItemProperty property = item.Properties.GetProperty(dae.Name);
				if (property != null && property.IsEvent) {
					return property;
				}
			}
			return null;
		}

		void GenerateName(DesignItem item)
		{
			for (int i = 1; ; i++) {
				try {
					string name = item.ComponentType.Name + i;
					name = char.ToLower(name[0]) + name.Substring(1);
					item.Name = name;
					break;
				}
				catch {
				}
			}
		}
	}
}