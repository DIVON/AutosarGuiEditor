using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutosarGuiEditor.Source.Utility
{
    public static class MessageUtilities
    {
        public static bool AskToDelete(string message)
        {
            MessageBoxResult result = MessageBox.Show(message, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
    }
}
