// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;

namespace ICSharpCode.PythonBinding
{
	/// <summary>
	/// Represents a root component in the designer.
	/// </summary>
	public class PythonDesignerRootComponent : PythonDesignerComponent
	{
		string rootNamespace = String.Empty;

		public PythonDesignerRootComponent(IComponent component)
			: this(component, String.Empty)
		{
		}
		
		public PythonDesignerRootComponent(IComponent component, string rootNamespace)
			: base(null, component)
		{
			this.rootNamespace = rootNamespace;
		}
		
		public override string GetPropertyOwnerName()
		{
			return "self";
		}
		
		public override void AppendSuspendLayout(PythonCodeBuilder codeBuilder)
		{
			AppendMethodCalls(codeBuilder, suspendLayoutMethods);
		}
		
		public override void AppendResumeLayout(PythonCodeBuilder codeBuilder)
		{
			AppendMethodCalls(codeBuilder, resumeLayoutMethods);
		}		
		
		public override void AppendComponent(PythonCodeBuilder codeBuilder)
		{
			// Add the child components first.
			foreach (PythonDesignerComponent component in GetContainerComponents()) {
				component.AppendComponent(codeBuilder);
			}
			
			// Add root component
			AppendComponentProperties(codeBuilder, false, true);
			if (HasAddedResources()) {
				InsertCreateResourceManagerLine(codeBuilder);
			}
		}
		
		/// <summary>
		/// Adds BeginInit method call for any components that implement the 
		/// System.ComponentModel.ISupportInitialize interface.
		/// </summary>
		public void AppendSupportInitializeComponentsBeginInit(PythonCodeBuilder codeBuilder)
		{
			AppendSupportInitializeMethodCalls(codeBuilder, new string[] {"BeginInit()"});
		}
		
		/// <summary>
		/// Adds EndInit method call for any that implement the 
		/// System.ComponentModel.ISupportInitialize interface.
		/// </summary>
		public void AppendSupportInitializeComponentsEndInit(PythonCodeBuilder codeBuilder)
		{
			AppendSupportInitializeMethodCalls(codeBuilder, new string[] {"EndInit()"});
		}
		
		public void AppendSupportInitializeMethodCalls(PythonCodeBuilder codeBuilder, string[] methods)
		{
			foreach (PythonDesignerComponent component in GetContainerComponents()) {
				if (typeof(ISupportInitialize).IsAssignableFrom(component.GetComponentType())) {
					component.AppendMethodCalls(codeBuilder, methods);
				}
			}
		}
				
		/// <summary>
		/// Reverses the ordering when adding items to the Controls collection.
		/// </summary>
		public override void AppendMethodCallWithArrayParameter(PythonCodeBuilder codeBuilder, string propertyOwnerName, object propertyOwner, PropertyDescriptor propertyDescriptor)
		{
			bool reverse = propertyDescriptor.Name == "Controls";
			AppendMethodCallWithArrayParameter(codeBuilder, propertyOwnerName, propertyOwner, propertyDescriptor, reverse);
		}
		
		/// <summary>
		/// Writes resources to file.
		/// </summary>
		public void GenerateResources(IResourceService resourceService)
		{
			if (resourceService == null) {
				return;
			}
			
			using (IResourceWriter writer = resourceService.GetResourceWriter(CultureInfo.InvariantCulture)) {
				foreach (PythonDesignerComponent component in GetContainerComponents()) {
					component.GenerateResources(writer);
				}
				GenerateResources(writer);
			}
		}
		
		public string GetResourceRootName()
		{
			string componentName = Component.Site.Name;
			if (!String.IsNullOrEmpty(rootNamespace)) {
				return rootNamespace + "." + componentName;
			}
			return componentName;
		}
		
		void InsertCreateResourceManagerLine(PythonCodeBuilder codeBuilder)
		{
			StringBuilder line = new StringBuilder();
			line.Append("resources = System.Resources.ResourceManager(\"");
			line.Append(GetRootComponentRootResourceName());
			line.Append("\", System.Reflection.Assembly.GetEntryAssembly())");
			codeBuilder.InsertIndentedLine(line.ToString());
		}
		
		string GetRootComponentRootResourceName()
		{
			PythonDesignerComponent component = this;
			while (component != null) {
				if (component.Parent == null) {
					PythonDesignerRootComponent rootComponent = component as PythonDesignerRootComponent;
					return rootComponent.GetResourceRootName();
				}
				component = component.Parent;
			}
			return String.Empty;
		}
		
		/// <summary>
		/// Returns true if a resource has been added to any of the form components.
		/// </summary>
		bool HasAddedResources()
		{
			if (HasResources) {
				return true;
			}
			
			foreach (PythonDesignerComponent component in GetContainerComponents()) {
				if (component.HasResources) {
					return true;
				}
			}
				
			return false;
		}
	}
}