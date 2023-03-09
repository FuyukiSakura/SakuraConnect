
using BlazorBootstrap;

namespace Sakura.Live.Connect.Dreamer.Models.Bootstrap
{
    /// <summary>
    /// A list of default options for <see cref="ConfirmDialogOptions"/>
    /// </summary>
    public static class DialogOptions
    {
        /// <summary>
        /// Is shown when user tries to delete a setting
        /// </summary>
        public static readonly ConfirmDialogOptions ConfirmDelete = new()
        {
            YesButtonText = "Yes",
            YesButtonColor = ButtonColor.Danger,
            NoButtonText = "Cancel",
            NoButtonColor = ButtonColor.Light
        };
    }
}
