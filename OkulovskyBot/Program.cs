
//6597258627:AAH9RFHxr3DD2KYGhH-9fZMYdArCsmp8uVs

using System;
using System.Diagnostics;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OkulovskyBot
{
    class HelloWorld
    {
        static void Main()
        {
            var okula = new Bot();
            okula.Update();
        }
    }

    public static class BotDialogs
    {

    }
        

    public class Bot
    {
        private const string token = "6597258627:AAH9RFHxr3DD2KYGhH-9fZMYdArCsmp8uVs";

        public TelegramBotClient bot;
        public IStatusBotVisitor state { get; private set; }

        public Bot()
        {
            bot = new TelegramBotClient(token);
            state = new StartVisitor();
        }



        public void Update()
        {
            var offset = 0;
            while (true)
            {
                var updates = bot.GetUpdatesAsync(offset).Result;
                foreach (var update in updates)
                {
                    ProcessUpdate(update);
                    offset = (update.Id + 1);
                    Console.WriteLine(update.Id);
                }
                Thread.Sleep(1000);          
            }         
        }


        public void ProcessUpdate(Telegram.Bot.Types.Update update)
        {
            state.Process(update, bot);           
        }
    }

    public interface IStatusBotVisitor
    {
        public void Process(Telegram.Bot.Types.Update update, TelegramBotClient telegramBot);
    }

    public class StartVisitor : IStatusBotVisitor
    {
        public void Process(Update update, TelegramBotClient bot)
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

        public void ProcessMessage(Update update, TelegramBotClient bot)
        {
            if (update.Message.Text == "/start")
                bot.SendTextMessageAsync(update.Message.Chat.Id, "Вперёд");
            else if (update.Message.Text == "/help")
                bot.SendTextMessageAsync(update.Message.Chat.Id, "НитЯТУПОЙ");
            else
                bot.SendTextMessageAsync(update.Message.Chat.Id, "Mesedge");
        }
    }
}

