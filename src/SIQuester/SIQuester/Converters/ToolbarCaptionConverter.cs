using SIQuester.ViewModel;
using System.Globalization;
using System.Threading;
using System.Windows.Data;

namespace SIQuester.Converters;

/// <summary>
/// Gives a short caption for an action toolbar row (what object the row buttons act on).
/// </summary>
public sealed class ToolbarCaptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var ru = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "ru";

        return value switch
        {
            PackageViewModel => Equals(parameter, "item") ? (ru ? "Логотип" : "Logo") : (ru ? "Пакет" : "Package"), // second row: logo actions
            RoundViewModel => ru ? "Раунд" : "Round",
            ThemeViewModel => ru ? "Тема" : "Theme",
            QuestionViewModel => ru ? "Вопрос" : "Question",
            AnswersViewModel answers => answers.IsRight ? (ru ? "Ответы" : "Answers") : (ru ? "Неверные" : "Wrong"),
            ContentItemsViewModel content => IsComplexAnswer(content) || !content.IsTopLevel
                ? (ru ? "Ответ" : "Answer")
                : (ru ? "Текст вопроса" : "Question body"),
            TagsViewModel => ru ? "Теги" : "Tags",
            AuthorsViewModel => ru ? "Авторы" : "Authors",
            SourcesViewModel => ru ? "Источники" : "Sources",
            LinksViewModel => ru ? "Ссылки" : "Links",
            _ => ""
        };
    }

    private static bool IsComplexAnswer(ContentItemsViewModel content) =>
        content.Owner.Parameters?.Any(p => p.Key == SIPackages.Core.QuestionParameterNames.Answer && p.Value.ContentValue == content) == true;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
