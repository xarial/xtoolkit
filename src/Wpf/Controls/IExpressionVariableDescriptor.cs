using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xarial.XToolkit.Services.Expressions;

namespace Xarial.XToolkit.Wpf.Controls
{
    public interface IExpressionVariableDescriptor 
    {
        string GetTitle(IExpressionTokenVariable variable);
        ImageSource GetIcon(IExpressionTokenVariable variable);
        string GetDescription(IExpressionTokenVariable variable);
        Brush GetBackground(IExpressionTokenVariable variable);
        ExpressionVariableArgumentDescriptor[] GetArguments(IExpressionTokenVariable variable, out bool dynamic);
    }

    public class ExpressionVariableArgumentsInfo 
    {
        public string VariableName { get; set; }

        public bool Dynamic { get; set; }

        public string TitleFormat { get; set; }
        public ImageSource Icon { get; set; }
        public string DescriptionFormat { get; set; }
        public Brush Background { get; set; }
        public Collection<ExpressionVariableArgumentDescriptor> Arguments { get; }

        public ExpressionVariableArgumentsInfo() 
        {
            Arguments = new Collection<ExpressionVariableArgumentDescriptor>();
        }
    }

    public class ExpressionVariableDescriptor : Collection<ExpressionVariableArgumentsInfo>, IExpressionVariableDescriptor
    {
        private readonly StringComparison m_Comparison;

        public ExpressionVariableDescriptor() : this(StringComparison.CurrentCultureIgnoreCase)
        {
        }

        public ExpressionVariableDescriptor(StringComparison comparison) 
        {
            m_Comparison = comparison;
        }

        public ExpressionVariableArgumentDescriptor[] GetArguments(IExpressionTokenVariable variable, out bool dynamic)
        {
            var info = GetInfo(variable);

            dynamic = info.Dynamic;

            return info.Arguments?.ToArray();
        }

        public Brush GetBackground(IExpressionTokenVariable variable) => GetInfo(variable).Background;
        public string GetDescription(IExpressionTokenVariable variable) => string.Format(GetInfo(variable).DescriptionFormat, GetArgumentsArray(variable));
        public ImageSource GetIcon(IExpressionTokenVariable variable) => GetInfo(variable).Icon;
        public string GetTitle(IExpressionTokenVariable variable) => string.Format(GetInfo(variable).TitleFormat, GetArgumentsArray(variable));

        private ExpressionVariableArgumentsInfo GetInfo(IExpressionTokenVariable variable) 
        {
            var info = this.FirstOrDefault(i => string.Equals(i.VariableName, variable.Name, m_Comparison));

            if (info == null) 
            {
                throw new NullReferenceException($"Failed to find the information about the variable '{variable.Name}'");
            }

            return info;
        }

        private string[] GetArgumentsArray(IExpressionTokenVariable variable)
        {
            var args = variable.Arguments?.Select(a =>
            {
                if (a is IExpressionTokenText)
                {
                    return ((IExpressionTokenText)a).Text;
                }
                else
                {
                    return "{}";
                }
            })?.ToArray() ?? new string[0];

            var info = GetInfo(variable);

            if (args.Length < info.Arguments.Count)
            {
                args = args.Concat(Enumerable.Repeat("", info.Arguments.Count - args.Length)).ToArray();
            }

            return args;
        }
    }
}
