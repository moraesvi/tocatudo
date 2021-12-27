using System.Windows.Input;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim.Effects
{
    public class LongPressedEffect : RoutingEffect
    {
        public LongPressedEffect() : base("MyApp.LongPressedEffect")
        {
        }

        public static readonly BindableProperty CommandProperty = BindableProperty.CreateAttached("Command", typeof(ICommand), typeof(LongPressedEffect), (object)null);
        public static Command GetCommand(BindableObject view)
        {
            return (Command)view.GetValue(CommandProperty);
        }

        public static void SetCommand(BindableObject view, bool value)
        {
            view.SetValue(CommandProperty, value);
        }


        public static readonly BindableProperty CommandParameterProperty = BindableProperty.CreateAttached("CommandParameter", typeof(object), typeof(LongPressedEffect), (object)null);
        public static Command GetCommandParameter(BindableObject view)
        {
            return (Command)view.GetValue(CommandParameterProperty);
        }

        public static void SetCommandParameter(BindableObject view, bool value)
        {
            view.SetValue(CommandParameterProperty, value);
        }
    }
}
