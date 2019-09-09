using System;
using System.Windows.Input;

namespace BatchProcessorServerUI.Util
{
    /// <summary>
    /// Simple Button Command Class to call an Action Delegate
    /// </summary>
    public class ButtonCommand : ICommand
    {
        private Action buttonDelegate;
        
        public ButtonCommand(Action buttonDelegate)
        {
            this.buttonDelegate = buttonDelegate;
        }

        // Interface Implementation

        public event EventHandler CanExecuteChanged = (asender, e) => { };

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            buttonDelegate();
        }
    }
}
