using ImageClassifier.Core.Interfaces;

namespace ImageClassifier.Infrastructure.Services
{
    public class MauiDialogService : IDialogService
    {
        public async Task DisplayAlert(string title, string message, string cancel)
        {
            if (Application.Current?.Windows[0].Page is Page mainPage)
                await mainPage.DisplayAlert(title, message, cancel);
        }

        public async Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
        {
            if (Application.Current?.Windows[0].Page is Page mainPage)
                return await mainPage.DisplayAlert(title, message, accept, cancel);
            return false;
        }
    }
}
