using System;
using System.Diagnostics;
using System.Drawing;
using System.Security.Principal;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Graph.Int;
using System.Text;
using Graph.App;
using OkulovskyBot;
using Graph.Dom.Algoritms;
using System.IO;
using Graph.Dom;
using File = Telegram.Bot.Types.File;

namespace Graph.Int
{
    public class BaseData
    {
        public List<BuilderImplementation> builder = new List<BuilderImplementation>();

        private string connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=Okulovsky2024;Integrated Security=True";

        private DatabaseHelper databaseHelper;

        public BaseData()
        {
            databaseHelper = new DatabaseHelper(connectionString);
        }

        public void SaveToDatabase(BuilderImplementation implementation)
        {
            databaseHelper.InsertBuilderImplementation(implementation);
        }

        public void LoadFromDatabase()
        {
            builder = databaseHelper.GetAllBuilderImplementations();
        }

        public List<BuilderImplementation> GetImplementationsFromDatabaseByName(string name)
        {
            return databaseHelper.GetBuilderImplementationsByName(name);
        }

        public List<BuilderImplementation> GetMyImplementations(string authorName)
        {
            return databaseHelper.GetBuilderImplementationsByAuthorName(authorName);
        }

        public Reactions GetReactionsById(int id)
        {
            return databaseHelper.GetReactionsById(id);
        }

        public void UpdateReactions(int id, int likes, int dislikes)
        {
            databaseHelper.UpdateReactions(id, likes, dislikes);
        }
    }

    public class Account
    {
        public string Name { get; private set; }
        public long Id { get; private set; }


        public Account(string name, long id)
        {
            Name = name; Id = id;
        }

        public static Account? ParseUpdateInAccount(Update update)
        {
            var u = update.Type;
            if (update.Message != null)
                return new Account(update.Message.Chat.FirstName, update.Message.Chat.Id);
            else if (update.CallbackQuery != null)
                return new Account(update.CallbackQuery.Message.Chat.FirstName, update.CallbackQuery.Message.Chat.Id);
            return null;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() * 137 + Id.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is Account)
                return Equals((Account)obj);
            return false;
        }

        public bool Equals(Account obj)
        {
            return Name == obj.Name && Id == obj.Id;
        }

    }


    public static class Infrastructure
    {

        public static ReplyKeyboardMarkup GetMenuButtons()
        {
            var markup = new ReplyKeyboardMarkup
            (
                new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton> { new KeyboardButton("Найти решение"), new KeyboardButton("Добавить решение")},
                    new List<KeyboardButton> { new KeyboardButton("Визуализировать граф"), new KeyboardButton("Мои реализации") }
                }

            );
            markup.ResizeKeyboard = true;
            return markup;
        }
        public static InlineKeyboardMarkup GetInlineAddTypeButton()
        {
            return new InlineKeyboardMarkup(
                new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Алгоритм"),
                        InlineKeyboardButton.WithCallbackData("Метод"),

                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Класс"),
                        InlineKeyboardButton.WithCallbackData("Структура"),
                    }
                });
        }

        public static ReplyKeyboardMarkup GetProcesButtons()
        {
            var markup = new ReplyKeyboardMarkup
            (
                new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton> { new KeyboardButton("Помощь"), new KeyboardButton("Назад")},
                }
            );
            markup.ResizeKeyboard = true;
            return markup;
        }
    }


    public class Bot
    {
        private const string token = "6597258627:AAH9RFHxr3DD2KYGhH-9fZMYdArCsmp8uVs";

        public TelegramBotClient botTelegram;

        public Dictionary<Account, IStatusBotVisitor> accountsData;

        public BaseData data;

        public Bot()
        {
            botTelegram = new TelegramBotClient(token);
            accountsData = new Dictionary<Account, IStatusBotVisitor>();
            data = new BaseData();
        }



        public void Update()
        {
            var offset = 0;
            while (true)
            {
                var updates = botTelegram.GetUpdatesAsync(offset).Result;
                foreach (var update in updates)
                {
                    ProcessUpdate(update);
                    offset = update.Id + 1;
                }
                Thread.Sleep(100);
            }
        }


        public void ProcessUpdate(Update update)
        {
            var account = Account.ParseUpdateInAccount(update);
            if (account == null)
                return;

            if (accountsData.ContainsKey(account))
            {
                accountsData[account].Process(update, this);
            }
            else
            {
                accountsData.Add(account, new StartState());
                accountsData[account].Process(update, this);
            }
        }
    }



    public interface IStatusBotVisitor
    {
        public void Process(Update update, Bot telegramBot);
    }

    public class StartState : IStatusBotVisitor
    {

        public void Process(Update update, Bot bot)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    ProcessMessage(update, bot);
                    break;
                default:
                    break;
            }
        }

        public void ProcessMessage(Update update, Bot bot)
        {
            if (update.Message.Text == "/start")
            {
                bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Вперёд",
                    replyMarkup: Infrastructure.GetMenuButtons());
                var account = Account.ParseUpdateInAccount(update);
                bot.accountsData[account] = new BaseState();
            }
            else if (update.Message.Text == "/help")
                bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "НитЯТУПОЙ");
            else
                bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Mesedge");
        }

    }

    public class BaseState : IStatusBotVisitor
    {
        public void Process(Update update, Bot bot)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    ProcessMessage(update, bot);
                    break;
                default:
                    break;
            }
        }

        public void ProcessMessage(Update update, Bot bot)
        {
            var dictMesedge = new Dictionary<string, (string, IStatusBotVisitor?)>
            {
                ["Найти решение"] = ("Введите название реализации:", new FindState()),
                ["Добавить решение"] = ("Это настройщик добавляемого решения", new AddState()),
                ["Визуализировать граф"] = ("Выберите алгоритм для визуализации:\n1. Алгоритм Дейкстры\n2. Алгоритм Краскала", 
                new VisualizeState()),
                ["Мои реализации"] = ("Чтобы запустить поиск Ваших реализаций в базе, введите любую фразу:", new MyState(update, bot)),
            };
            if (update.Type != UpdateType.Message || update.Message.Text == null)
                return;
            if (dictMesedge.ContainsKey(update.Message.Text))
            {


                bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id,
                    dictMesedge[update.Message.Text].Item1, replyMarkup: Infrastructure.GetProcesButtons());
                if (update.Message.Text == "Добавить решение")
                    bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id,
                    "Выберите тип реализации из заданных вариантов:", replyMarkup: Infrastructure.GetInlineAddTypeButton());

                if (dictMesedge[update.Message.Text].Item2 != null)
                {
                    var account = new Account(update.Message.Chat.FirstName, update.Message.Chat.Id);
                    bot.accountsData[account] = dictMesedge[update.Message.Text].Item2;
                }
            }
            else
                bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Не понял вас");
        }
    }

    public enum TypeImplementation
    {
        Algorithm,
        Method,
        Class,
        Struct
    }

    public class BuilderImplementation
    {
        public int Id { get; set; }
        public TypeImplementation typeImplementation { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
    }

    public class Reactions
    {
        public int Likes { get; set; }
        public int Dislikes { get; set; }
    }

    public static class ButtonExtensions
    {
        public static bool ProcessingWrongMessage(this IStatusBotVisitor button, Update update, Bot bot, UpdateType type, string message)
        {
            if (update.Type == UpdateType.Message && update.Message.Text == null)
                return false;
            if (update.Type != type)
            {
                bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, message);
                return true;
            }
            return false;
        }

        public static bool TryProcessMenu(this IStatusBotVisitor button, Update update, Bot bot)
        {
            if (update.Message == null) return false;

            if (update.Message.Text == "Помощь")
            {
                bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Пожалуйста, следуйте инструкциям!");
                return true;
            }
            else if (update.Message.Text == "Назад")
            {
                bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Хорошо, возвращаемся назад!", replyMarkup: Infrastructure.GetMenuButtons());
                var account = Account.ParseUpdateInAccount(update);
                bot.accountsData[account] = new BaseState();
                return true;
            }
            return false;
        }
    }

    public class AddState : IStatusBotVisitor
    {
        BuilderImplementation builder { get; set; }

        List<Action<Update, Bot, BuilderImplementation>> substates;

        int pointerToSubstate;

        public AddState()
        {
            builder = new BuilderImplementation();
            substates = new List<Action<Update, Bot, BuilderImplementation>>
            {
                AddType, AddName, AddDescription, AddCode, AddInDataBase
            };
            pointerToSubstate = 0;
        }

        public void AddType(Update update, Bot bot, BuilderImplementation builder)
        {
            if (this.ProcessingWrongMessage(update, bot, UpdateType.CallbackQuery, "Необходимо ввести тип реализации!"))
                return;
            var dict = new Dictionary<string, TypeImplementation>
            {
                ["Алгоритм"] = TypeImplementation.Algorithm,
                ["Метод"] = TypeImplementation.Method,
                ["Класс"] = TypeImplementation.Class,
                ["Структура"] = TypeImplementation.Struct,
            };
            if (!dict.ContainsKey(update.CallbackQuery.Data))
            {
                bot.botTelegram.SendTextMessageAsync(update.CallbackQuery.From.Id, "Нажмите кнопку с предложенным вариантом");
                return;
            }
            builder.typeImplementation = dict[update.CallbackQuery.Data];
            bot.botTelegram.SendTextMessageAsync(update.CallbackQuery.From.Id, "Введите название Вашей реализации:");
            pointerToSubstate++;
        }

        public void AddName(Update update, Bot bot, BuilderImplementation builder)
        {
            if (this.ProcessingWrongMessage(update, bot, UpdateType.Message, "Необходимо ввести название реализации!"))
                return;
            builder.Name = update.Message.Text;
            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Введите описание Вашей реализации:");
            pointerToSubstate++;
        }

        public void AddDescription(Update update, Bot bot, BuilderImplementation builder)
        {
            if (this.ProcessingWrongMessage(update, bot, UpdateType.Message, "Необходимо ввести описание реализации!"))
                return;
            builder.Description = update.Message.Text;
            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Отправьте ответным сообщением код Вашей реализации на C#");
            pointerToSubstate++;
        }

        public void AddCode(Update update, Bot bot, BuilderImplementation builder)
        {
            if (this.ProcessingWrongMessage(update, bot, UpdateType.Message, "Необходимо ввести код реализации!"))
                return;
            builder.Code = update.Message.Text;
            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Согласны добавить реализацию?", replyMarkup: GetInlineButton());
            pointerToSubstate++;
        }

        public InlineKeyboardMarkup GetInlineButton()
        {
            return new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("Подтвердить") } });
        }

        public void AddInDataBase(Update update, Bot bot, BuilderImplementation builder)
        {
            if (this.ProcessingWrongMessage(update, bot, UpdateType.CallbackQuery, "AAA"))
                return;
            if (update.CallbackQuery.Data == "Подтвердить")
            {
                bot.botTelegram.SendTextMessageAsync(update.CallbackQuery.From.Id,
                    "Отлично! Реализация принята и успешно загружена.\n\nБлагодарим Вас за пополнение базы! Спасибо, что помогаете другим в изучении C# ♥", replyMarkup: Infrastructure.GetMenuButtons());
                var authorName = $"{update.CallbackQuery.From.FirstName} {update.CallbackQuery.From.LastName}";
                builder.Author = authorName;
            }   
            bot.data.SaveToDatabase(builder);
            var account = Account.ParseUpdateInAccount(update);
            bot.accountsData[account] = new BaseState();
        }

        public void Process(Update update, Bot bot)
        {
            if (!this.TryProcessMenu(update, bot))
                substates[pointerToSubstate](update, bot, builder);
        }
    }

    public class FindState : IStatusBotVisitor
    {
        List<Action<Update, Bot>> substates;
        private List<BuilderImplementation> implementations;
        private int number;

        int pointerToSubstate;

        public FindState()
        {
            substates = new List<Action<Update, Bot>>
            {
                GetImplementationsFromDatabase, RepresentImplementation, MaintainButtonReactions,
                RepresentImplementationWithDefiniteNumber, UpdateImplementationLike, UpdateImplementationDisLike
            };
            pointerToSubstate = 0;
        }

        public void GetImplementationsFromDatabase(Update update, Bot bot)
        {
            if (this.ProcessingWrongMessage(update, bot, UpdateType.Message, "Необходимо ввести название реализации"))
                return;

            var name = update.Message.Text;
            implementations = bot.data.GetImplementationsFromDatabaseByName(name);

            if (implementations.Count == 0)
            {
                bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Не удалось найти реализации по указанному названию...");
                return;
            }

            var message = new StringBuilder();
            message.AppendLine("Вот что мне удалось найти по Вашему запросу:\n");

            for (var i = 0; i < implementations.Count; i++)
            {
                var implementation = implementations[i];
                message.AppendLine($"{i + 1}. {implementation.Name}, автор: {implementation.Author}");
            }

            message.AppendLine("\nВ ответном сообщении напишите порядковый номер, и я поделюсь всей информацией по данной реализации!");

            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, message.ToString());

            pointerToSubstate++;
        }

        public InlineKeyboardMarkup GetReactionButtons(Reactions reactions)
        {
            return new InlineKeyboardMarkup(
                new[] { new[] { InlineKeyboardButton.WithCallbackData($"{reactions.Likes} likes"), 
                InlineKeyboardButton.WithCallbackData($"{reactions.Dislikes} dislikes"),
                InlineKeyboardButton.WithCallbackData("Next variant")}});
        }

        public InlineKeyboardMarkup GetNextButton()
        {
            return new InlineKeyboardMarkup(
                new[] { new[] { InlineKeyboardButton.WithCallbackData("Next variant")}});
        }

        public void RepresentImplementation(Update update, Bot bot)
        {
            if (this.ProcessingWrongMessage(update, bot, UpdateType.Message, "Необходимо ввести порядковый номер"))
                return;
            number = int.Parse(update.Message.Text);
            var implementation = implementations[number - 1];
            var reactions = bot.data.GetReactionsById(implementation.Id);
            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id,
                $"{implementation.Name}\n\nАвтор: {implementation.Author}\n\n{implementation.Description}");
            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id,
                $"```\n{implementation.Code}\n```", parseMode: ParseMode.MarkdownV2, replyMarkup: GetReactionButtons(reactions));
            pointerToSubstate++;
        }

        public void MaintainButtonReactions(Update update, Bot bot)
        {
            if (this.ProcessingWrongMessage(update, bot, UpdateType.CallbackQuery, "AAA"))
                return;
            var message = update.CallbackQuery.Data;
            if (message.Contains("dislikes"))
            {
                bot.botTelegram.SendTextMessageAsync(update.CallbackQuery.From.Id,
                    "Введите любую фразу, если действительно хотите поставить данную оценку.");
                pointerToSubstate += 3;
            }
            else if (message.Contains("likes"))
            {
                bot.botTelegram.SendTextMessageAsync(update.CallbackQuery.From.Id,
                    "Введите любую фразу, если действительно хотите поставить данную оценку.");
                pointerToSubstate += 2;
            }
            if (message == "Next variant")
            {
                number++;
                bot.botTelegram.SendTextMessageAsync(update.CallbackQuery.From.Id,
                    "Уверены, что желаете перейти к следующей реализации? Введите любую фразу, иначе - нажмите \"Назад\" в меню!");
                pointerToSubstate++;
            }
        }

        public void RepresentImplementationWithDefiniteNumber(Update update, Bot bot)
        {
            if (number == implementations.Count + 1)
                number = 1;
            var implementation = implementations[number - 1];
            var reactions = bot.data.GetReactionsById(implementation.Id);
            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id,
                $"{implementation.Name}\n\nАвтор: {implementation.Author}\n\n{implementation.Description}");
            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id,
                $"```\n{implementation.Code}\n```", parseMode: ParseMode.MarkdownV2, replyMarkup: GetReactionButtons(reactions));
            pointerToSubstate--;
        }

        public void UpdateImplementationLike(Update update, Bot bot)
        {
            var currentImplementation = implementations[number - 1];
            var reactions = bot.data.GetReactionsById(currentImplementation.Id);
            bot.data.UpdateReactions(currentImplementation.Id, reactions.Likes + 1, reactions.Dislikes);
            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Оценка выставлена успешно", replyMarkup: GetNextButton());
            pointerToSubstate -= 2;
        }

        public void UpdateImplementationDisLike(Update update, Bot bot)
        {
            var currentImplementation = implementations[number - 1];
            var reactions = bot.data.GetReactionsById(currentImplementation.Id);
            bot.data.UpdateReactions(currentImplementation.Id, reactions.Likes, reactions.Dislikes + 1);
            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Оценка выставлена успешно", replyMarkup: GetNextButton());
            pointerToSubstate -= 3;
        }


        public void Process(Update update, Bot bot)
        {
            if (!this.TryProcessMenu(update, bot))
                substates[pointerToSubstate](update, bot);
        }
    }

    public class MyState : IStatusBotVisitor
    {
        List<Action<Update, Bot>> substates;
        private List<BuilderImplementation> implementations;
        int pointerToSubstate;

        public MyState(Update update, Bot bot)
        {
            substates = new List<Action<Update, Bot>>
            {
                GetMyImplementations, ShowDefiniteImplementation
            };
            pointerToSubstate = 0;
            implementations = bot.data.GetMyImplementations($"{update.Message.From.FirstName} {update.Message.From.LastName}");
        }

        public void GetMyImplementations(Update update, Bot bot)
        {
            if (implementations.Count == 0)
            {
                bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Вы не сохранили ещё ни одной реализации!");
                return;
            }

            var message = new StringBuilder();
            message.AppendLine("Ваши реализации:\n");

            for (var i = 0; i < implementations.Count; i++)
            {
                var implementation = implementations[i];
                message.AppendLine($"{i + 1}. {implementation.Name}");
            }

            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, message.ToString());

            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Введите порядковый номер реализации");
            pointerToSubstate++;
        }

        public InlineKeyboardMarkup GetChangeButtons()
        {
            return new InlineKeyboardMarkup(
                new[] { new[] { InlineKeyboardButton.WithCallbackData("Изменить"),
                InlineKeyboardButton.WithCallbackData("Удалить") }});
        }

        public void ShowDefiniteImplementation(Update update, Bot bot)
        {
            if (this.ProcessingWrongMessage(update, bot, UpdateType.Message, "Необходимо ввести порядковый номер"))
                return;
            var number = int.Parse(update.Message.Text);
            var implementation = implementations[number - 1];
            var reactions = bot.data.GetReactionsById(implementation.Id);
            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id,
                $"{implementation.Name}\n\n{reactions.Likes} Нравится | {reactions.Dislikes} Не нравится\n\n{implementation.Description}");
            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id,
                $"```\n{implementation.Code}\n```", parseMode: ParseMode.MarkdownV2, replyMarkup: GetChangeButtons());
        }

        public void Process(Update update, Bot bot)
        {
            if (!this.TryProcessMenu(update, bot))
                substates[pointerToSubstate](update, bot);
        }
    }

    public class VisualizeState : IStatusBotVisitor
    {
        List<Action<Update, Bot>> substates;
        int pointerToSubstate;
        private int desiredAlgorithm;
        private static HashSet<int> possibleAlgIndexes = new HashSet<int>() { 1, 2 };

        public VisualizeState()
        {
            substates = new List<Action<Update, Bot>>
            {
                ChooseAlgorithm,
                GetAdjacencyLists
            };
            pointerToSubstate = 0;
        }

        private async void ChooseAlgorithm(Update update, Bot bot)
        {
            try
            {
                desiredAlgorithm = int.Parse(update.Message.Text);
            }
            catch (FormatException e)
            {
                await bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Для указания алгоритма используйте только его номер");
                return;
            }
            if (!possibleAlgIndexes.Contains(desiredAlgorithm))
            {
                await bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Алгоритма с таким номером не существует");
                return;
            }
            await bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Введите cписки смежности");
            await bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Они указываются в формате\n" +
                                                                              "Номер вершины 'w'вес вершины ':' пары вида: номер вершины 'e'вес ребра\n" +
                                                                              "Указание веса вершин и ребер необязательны");
            await bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Пример корректного ввода\n" +
                                                                              "1 w42 : 2\n" +
                                                                              "2 w20 : 1\n" +
                                                                              "Создает граф с двунаправленным ребром 1-2, где вес 1 равен 42, а вес 2 равен 20");
            pointerToSubstate++;
        }

        private void  GetAdjacencyLists(Update update, Bot bot)
        {
            if (update.Message.Text == null)
                return;
            
            if (desiredAlgorithm == 1)
            {
                // Dijkstra
                var graph = GraphCreator.GetParsedGraph<N>(update.Message.Text, true);
                if (SendIfIncorrectGraphData(update, bot, graph))
                {
                    return;
                }
                var changes = Dijkstra.GetObserverForGraph(graph,
                    graph.Nodes.First().Id,graph.Nodes.Last().Id);
                var vis = new Visualizator<string, int, N>(changes, Dijkstra.DefineColor);
                CreateGifAndSendToUser(update, bot, vis);
            }
            if (desiredAlgorithm == 2)
            {
                // Kraskal
                var graph = GraphCreator.GetParsedGraph<State>(update.Message.Text);
                if (SendIfIncorrectGraphData(update, bot, graph))
                {
                    return;
                }
                var changes = Krascal.GetObserverForGraph(graph);
                var vis = new Visualizator<string, int, State>(changes,
                    (state) => ColorTranslator.FromHtml(state.ToString()));
                CreateGifAndSendToUser(update, bot, vis);
            }
        }

        private bool SendIfIncorrectGraphData<TState>(Update update, Bot bot, Graph<string, int, TState> graph)
        {
            if (graph == null)
            {
                bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Неверные данные для создания графа");
                return true;
            }
            return false;
        }

        private void CreateGifAndSendToUser<TId, TWeight, TState>(Update update, Bot bot, Visualizator<TId, TWeight, TState> vis)
        {
            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
            var path = Path.Combine(projectDirectory, "Gifs\\" + update.Message.Chat.Id.GetHashCode() + ".gif");
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            System.IO.File.Create(path).Close();
            vis.StartVisualize(25, path);
            using Stream stream = System.IO.File.OpenRead(path);
            bot.botTelegram.SendAnimationAsync(
                chatId: update.Message.Chat.Id,
                animation: new InputFileStream(stream, fileName: path));
        }

        public void Process(Update update, Bot bot)
        {
            if (!this.TryProcessMenu(update, bot))
                substates[pointerToSubstate](update, bot);
        }
    }
}
