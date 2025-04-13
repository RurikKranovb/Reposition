using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Reposition.Infrastructure;
using Reposition.Infrastructure.Interface;
using Tmds.DBus.Protocol;

namespace Reposition.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {

        [ObservableProperty] private string? _fileText;

        [RelayCommand]
        private async Task OpenFile(CancellationToken token)
        {
            ErrorMessages?.Clear();
            try
            {
                var filesService = App.Current?.Services?.GetService<IFileService>();
                if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

                var file = await filesService.OpenFileAsync();
                if (file is null) return;


                await using var readStream = await file.OpenReadAsync();
                using var reader = new StreamReader(readStream);
                FileText = await reader.ReadToEndAsync(token);


            }
            catch (Exception e)
            {
                ErrorMessages?.Add(e.Message);
            }
        }

        [RelayCommand]
        private void RepositionFile()
        {
            var fileText = FileText;
            var repositionRegex = new Regex(@"(?<=X)(-\d+\.?\d*)");
            var fileTextRegex = new Regex(@"(?<=X)(\d+\.?\d*)");

            if (fileText == null) return;

            var reposition = repositionRegex.Matches(fileText).SingleOrDefault()?.ToString()?.Replace("-", "");

            if (reposition == null) return;

            try
            {

                var list = fileTextRegex.Matches(fileText).ToList();

                foreach (var item in list)
                {
                    var index = fileText.IndexOf(item.ToString(), StringComparison.Ordinal);

                    //double.TryParse(item.Value, out var value);
                    //double.TryParse(reposition, out var repositionValue);

                    var value = double.Parse(item.Value.Replace(".", ","));
                    var repositionValue = double.Parse(reposition.Replace(".", ","));

                    var replaceItem = Math.Round(value - repositionValue, 2);


                    fileText = fileText.Remove(index, item.Length)
                        .Insert(index, replaceItem.ToString(CultureInfo.InvariantCulture));

                }

                FileText = fileText;
            }
            catch (Exception e)
            {
                ErrorMessages?.Add(e.Message);
            }

        }

        [RelayCommand]
        private async Task SaveFile()
        {
            ErrorMessages?.Clear();
            try
            {
                var filesService = App.Current?.Services?.GetService<IFileService>();
                
                if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

                var file = await filesService.SaveFileAsync();
                
                if (file is null) return;

                var stream = new MemoryStream(Encoding.Default.GetBytes((string)FileText));
                await using var writeStream = await file.OpenWriteAsync();
                await stream.CopyToAsync(writeStream);

            }
            catch (Exception e)
            {
                ErrorMessages?.Add(e.Message);
            }
        }
    }
}
