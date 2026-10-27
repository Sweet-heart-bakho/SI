using System.Globalization;

namespace SIQuester.ViewModel;

/// <summary>
/// Texts of the modernized UI (assistant chat, answer fields, text size, theme switch) in Russian and English.
/// The language follows the program UI language.
/// </summary>
public static class UiText
{
    private static bool IsRussian => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru";

    private static string T(string ru, string en) => IsRussian ? ru : en;

    // Theme progress badge
    public static string BadgeQuestions => T("вопросы", "questions");
    public static string BadgeAnswers => T("  ·  ответы", "  ·  answers");
    public static string BadgeToolTip => T("Заполнено вопросов / вопросов с правильным ответом", "Questions with text / questions with a right answer");

    // Info fields
    public static string RemoveAuthors => T("Удалить авторов", "Remove authors");
    public static string RemoveSources => T("Удалить источники", "Remove sources");

    // Tags window
    public static string TagsSelected => T("Выбранные теги", "Selected tags");
    public static string TagsNone => T("Пока ничего не выбрано — щёлкните тег ниже или введите свой", "Nothing selected yet — click a tag below or type your own");
    public static string TagsPlaceholder => T("Найти или добавить тег… (Enter — добавить)", "Find or add a tag… (Enter — add)");
    public static string TagsAdd => T("Добавить", "Add");

    // Creation date
    public static string Today => T("Сегодня", "Today");
    public static string TodayTip => T("Поставить сегодняшнюю дату", "Set today's date");

    // Hints in empty package fields
    public static string HintRestriction => T("например, 12+ или 18+", "e.g. 12+ or 18+");
    public static string HintContact => T("сайт, почта или Telegram автора", "author's site, e-mail or Telegram");
    public static string HintPublisher => T("кто выпустил пакет", "who published the pack");

    // Round type
    public static string RoundTypeHint => T(
        "Тип раунда — как он пройдёт в игре.\n" +
        "Таблица: табло «темы × стоимости», игроки выбирают вопросы и жмут кнопку (обычно 5 вопросов в теме).\n" +
        "Список тем: игроки по очереди убирают темы, по последней оставшейся все делают ставки и пишут ответ (1 вопрос в теме).",
        "Round type — how the round is played.\n" +
        "Table: a board of themes × prices, players pick questions and press the button (usually 5 questions per theme).\n" +
        "Theme list: players remove themes in turn; on the last one everyone bets and writes an answer (1 question per theme).");

    // Answer fields
    public static string AddAnswer => T("Добавить ответ (Enter)", "Add answer (Enter)");
    public static string RemoveAnswer => T("Удалить ответ", "Remove answer");
    public static string WrongAnswersTextOnly => T(
        "Неверные ответы могут быть только текстом. Картинку, звук или видео можно вставить в вопрос или в правильный ответ.",
        "Wrong answers can only be text. Paste pictures, sound or video into the question or the right answer.");

    // Command bar
    public static string FontSmaller => T("Уменьшить текст (Ctrl+−)", "Smaller text (Ctrl+−)");
    public static string FontBigger => T("Увеличить текст (Ctrl++)", "Larger text (Ctrl++)");
    public static string FontReset => T("Вернуть размер текста по умолчанию (Ctrl+0)", "Default text size (Ctrl+0)");
    public static string FontUnit => T(" пт", " pt");
    public static string SearchPlaceholder => T("Поиск по пакету", "Search the pack");
    public static string ToggleTheme => T("Светлая / тёмная тема", "Light / dark theme");

    // Settings
    public static string ClaudeHint => T(
        "Ключ можно получить на platform.claude.com (раздел API keys). Он хранится в зашифрованном виде только для вашей учётной записи Windows. " +
        "Если ключ задан, генерация вопросов и тем идёт через Claude (модель claude-opus-5-5), а в правом нижнем углу работает чат-помощник.",
        "Get a key at platform.claude.com (API keys). It is stored encrypted for your Windows account only. " +
        "When the key is set, question and theme generation uses Claude (claude-opus-5-5) and the assistant chat in the bottom-right corner works.");

    // Assistant chat
    public static string ChatTitle => T("Claude — помощник", "Claude — assistant");
    public static string ChatSubtitle => T("Opus 5.5 · видит открытую тему и вопрос", "Opus 5.5 · sees the open theme and question");
    public static string ChatCollapse => T("Свернуть", "Minimize");
    public static string ChatNew => T("Новый разговор", "New conversation");
    public static string ChatSend => T("Отправить (Enter)", "Send (Enter)");
    public static string ChatStop => T("Остановить ответ", "Stop the answer");
    public static string ChatPlaceholder => T("Спросите Claude… (Shift+Enter — новая строка)", "Ask Claude… (Shift+Enter — new line)");
    public static string ChatThinking => T("Claude думает…", "Claude is thinking…");
    public static string ChatEmptyTitle => T("Чем помочь?", "How can I help?");
    public static string ChatEmptyHint => T(
        "Например: «проверь этот вопрос», «придумай 5 неверных ответов», «предложи вопрос на 500 по этой теме», «сделай формулировку короче».",
        "For example: “check this question”, “suggest 5 wrong answers”, “write a 500-point question for this theme”, “make the wording shorter”.");
    public static string ChatButton => T("Claude — помощник", "Claude — assistant");

    public static string ChatNoKey => T(
        "Чтобы я мог отвечать, укажите ключ Claude API: Параметры → Claude (Anthropic) → Claude API key.",
        "To let me answer, set the Claude API key: Settings → Claude (Anthropic) → Claude API key.");

    public static string ChatTitleTail => T(" — помощник", " — assistant");
    public static string ChatSubtitleTail => T(" · видит открытую тему и вопрос", " · sees the open theme and question");

    public static string ChatNoKeyGpt => T(
        "Чтобы ChatGPT мог отвечать, укажите ключ OpenAI: Параметры → GPT → API key.",
        "To let ChatGPT answer, set the OpenAI key: Settings → GPT → API key.");

    // Settings: assistant choice
    public static string AiProviderHeader => T("ИИ-помощник", "AI assistant");
    public static string AiProviderLabel => T("Кто работает в чате и генерирует вопросы:", "Who answers in the chat and generates questions:");
    public static string AiProviderClaude => T("Claude (Anthropic) — модель Opus 5.5", "Claude (Anthropic) — Opus 5.5 model");
    public static string AiProviderGpt => T("ChatGPT (OpenAI) — модель и ключ из раздела GPT ниже", "ChatGPT (OpenAI) — model and key from the GPT section below");

    public static string ChatRefused => T("Claude отказался отвечать на этот запрос. Попробуйте переформулировать.", "Claude declined this request. Try rephrasing it.");
    public static string ChatTruncated => T("(ответ обрезан: слишком длинный)", "(answer cut: too long)");
    public static string ChatStopped => T("(остановлено)", "(stopped)");
    public static string ChatErrorPrefix => T("Ошибка: ", "Error: ");

    // Claude API errors
    public static string ClaudeNoKey => T(
        "Не задан ключ Claude API (Параметры → Claude API key или переменная ANTHROPIC_API_KEY).",
        "Claude API key is not set (Settings → Claude API key or the ANTHROPIC_API_KEY variable).");

    public static string ClaudeRefused => T("Claude отказался выполнить запрос. Попробуйте переформулировать тему или вопрос.", "Claude declined the request. Try rephrasing the theme or the question.");
    public static string ClaudeTooLong => T("Ответ Claude оказался слишком длинным и был обрезан. Попробуйте ещё раз.", "Claude's answer was too long and got cut. Please try again.");
}
