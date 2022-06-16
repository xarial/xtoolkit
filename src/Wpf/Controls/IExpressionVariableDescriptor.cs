using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xarial.XToolkit.Services.Expressions;

namespace Xarial.XToolkit.Wpf.Controls
{
    public interface IExpressionVariableDescriptor 
    {
        string GetTitle(IExpressionTokenVariable variable);
        BitmapImage GetIcon(IExpressionTokenVariable variable);
        string GetDescription(IExpressionTokenVariable variable);
        Brush GetBackground(IExpressionTokenVariable variable);
        ExpressionVariableArgumentDescriptor[] GetArguments(IExpressionTokenVariable variable, out bool dynamic);
    }
}
