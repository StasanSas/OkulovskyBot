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
using File = Telegram.Bot.Types.File;

namespace Graph.Int
{
    public class BaseData
    {
        public List<BuilderImplementation> builder = new List<BuilderImplementation>();

        private string connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=Okulovsky100;Integrated Security=True";

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
                ["Найти решение"] = ("Напишите название искомой реализации", new FindState()),
                ["Добавить решение"] = ("Введите тип реализации", new AddState()),
                ["Визуализировать граф"] = ("Выберите алгоритм для визуализации:\n1. Алгоритм Дейкстры\n2. Алгоритм Краскала", 
                new VisualizeState()),
                ["Мои реализации"] = ("Поищем...\nПодтвердите, что Вы не робот. Сложите 2+2", new MyState()),
            };
            if (update.Type != UpdateType.Message || update.Message.Text == null)
                return;
            if (dictMesedge.ContainsKey(update.Message.Text))
            {


                bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id,
                    dictMesedge[update.Message.Text].Item1, replyMarkup: Infrastructure.GetProcesButtons());
                if (update.Message.Text == "Добавить решение")
                    bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id,
                    "Из заданных вариантов", replyMarkup: Infrastructure.GetInlineAddTypeButton());

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
        public TypeImplementation typeImplementation { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
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
                bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Даже Окуловский вам не поможет");
                return true;
            }
            else if (update.Message.Text == "Назад")
            {
                bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Ну ладно", replyMarkup: Infrastructure.GetMenuButtons());
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
            if (this.ProcessingWrongMessage(update, bot, UpdateType.CallbackQuery, "Необходимо ввести тип"))
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
            bot.botTelegram.SendTextMessageAsync(update.CallbackQuery.From.Id, "Введите имя реализации");
            pointerToSubstate++;
        }

        public void AddName(Update update, Bot bot, BuilderImplementation builder)
        {
            if (this.ProcessingWrongMessage(update, bot, UpdateType.Message, "Необходимо ввести название"))
                return;
            builder.Name = update.Message.Text;
            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Напишите описание реализации");
            pointerToSubstate++;
        }

        public void AddDescription(Update update, Bot bot, BuilderImplementation builder)
        {
            if (this.ProcessingWrongMessage(update, bot, UpdateType.Message, "Необходимо ввести описание"))
                return;
            builder.Description = update.Message.Text;
            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Напишите код реализации");
            pointerToSubstate++;
        }

        public void AddCode(Update update, Bot bot, BuilderImplementation builder)
        {
            if (this.ProcessingWrongMessage(update, bot, UpdateType.Message, "Необходимо ввести код"))
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
                bot.botTelegram.SendTextMessageAsync(update.CallbackQuery.From.Id, "Реализация добавлена в базу", replyMarkup: Infrastructure.GetMenuButtons());
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

        int pointerToSubstate;

        public FindState()
        {
            substates = new List<Action<Update, Bot>>
            {
                GetImplementationsFromDatabase, RepresentImplementation
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
                bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Не удалось найти реализации по указанному названию");
                return;
            }

            var message = new StringBuilder();
            message.AppendLine("Вот какие реализации мне удалось найти по Вашему запросу:\n");

            for (var i = 0; i < implementations.Count; i++)
            {
                var implementation = implementations[i];
                message.AppendLine($"{i + 1}. {implementation.Name}, автор: {implementation.Author}");
            }

            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, message.ToString());

            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Введите порядковый номер реализации");
            pointerToSubstate++;
        }

        public void RepresentImplementation(Update update, Bot bot)
        {
            if (this.ProcessingWrongMessage(update, bot, UpdateType.Message, "Необходимо ввести порядковый номер"))
                return;
            var number = int.Parse(update.Message.Text);
            var implementation = implementations[number - 1];
            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id,
                $"{implementation.Name}\n\n{implementation.Description}\n\n{implementation.Code}");
            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Вот такая клёвая реализация", replyMarkup: Infrastructure.GetMenuButtons());
            var account = Account.ParseUpdateInAccount(update);
            bot.accountsData[account] = new BaseState();
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

        public MyState()
        {
            substates = new List<Action<Update, Bot>>
            {
                GetMyImplementations, ShowDefiniteImplementation
            };
            pointerToSubstate = 0;
        }

        public void GetMyImplementations(Update update, Bot bot)
        {
            var fullName = $"{update.Message.From.FirstName} {update.Message.From.LastName}";
                
            implementations = bot.data.GetMyImplementations(fullName);

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

        public void ShowDefiniteImplementation(Update update, Bot bot)
        {
            if (this.ProcessingWrongMessage(update, bot, UpdateType.Message, "Необходимо ввести порядковый номер"))
                return;
            var number = int.Parse(update.Message.Text);
            var implementation = implementations[number - 1];
            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id,
                $"{implementation.Name}\n\n{implementation.Description}\n\n{implementation.Code}");
            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Спасибо, что пользуетесь нашим ботом!", replyMarkup: Infrastructure.GetMenuButtons());
            var account = Account.ParseUpdateInAccount(update);
            bot.accountsData[account] = new BaseState();
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

        public VisualizeState()
        {
            substates = new List<Action<Update, Bot>>
            {
                ChooseAlgorithm,
                GetAdjacencyLists
            };
            pointerToSubstate = 0;
        }

        public void ChooseAlgorithm(Update update, Bot bot)
        {
            desiredAlgorithm = int.Parse(update.Message.Text);
            bot.botTelegram.SendTextMessageAsync(update.Message.Chat.Id, "Введите cписки смежности");
            pointerToSubstate++;
        }

        public void  GetAdjacencyLists(Update update, Bot bot)
        {
            if (update.Message.Text == null)
                return;
            
            if (desiredAlgorithm == 1)
            {
                // Dijkstra
                var graph = GraphCreator.GetParsedGraph<N>(update.Message.Text, true);
                var changes = Dijkstra.GetObserverForGraph(graph,
                    graph.Nodes.First().Id,graph.Nodes.Last().Id);
                var vis = new Visualizator<string, int, N>(changes, Dijkstra.DefineColor);
                CreateGifAndSendToUser(update, bot, vis);
            }
            if (desiredAlgorithm == 2)
            {
                // Kraskal
                var graph = GraphCreator.GetParsedGraph<State>(update.Message.Text);
                var changes = Krascal.GetObserverForGraph(graph);
                var vis = new Visualizator<string, int, State>(changes,
                    (state) => ColorTranslator.FromHtml(state.ToString()));
                CreateGifAndSendToUser(update, bot, vis);
            }
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
