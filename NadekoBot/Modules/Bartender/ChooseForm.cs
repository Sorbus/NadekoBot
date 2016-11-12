using Discord;
using Discord.Commands;
using Discord.Modules;
using NadekoBot.Classes;
using NadekoBot.Classes.JSONModels;
using NadekoBot.DataModels;
using NadekoBot.Extensions;
using NadekoBot.Modules.Permissions.Classes;
using System;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace NadekoBot.Modules.Bartender
{
    internal class ChooseForm : DiscordCommand
    {
        public static ConcurrentDictionary<long, TFQuestions> Active = new ConcurrentDictionary<long, TFQuestions>();

        public ChooseForm(DiscordModule module) : base(module)
        {
        }

        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Module.Prefix + "set")
                .Description($"Decide who you are. | `{Prefix}set`")
                .Do(async e =>
                {
                    if (Active.ContainsKey((long)e.User.Id))
                    {
                        await e.Channel.SendMessage($"Records show that you're in the middle of deciding, {e.User.Mention}. You can pick up where you left off.").ConfigureAwait(false);
                    }
                    var db = DbHandler.Instance.GetAllRows<UserMorph>();
                    Dictionary<long, UserMorph> morphs = db.Where(t => t.UserId.Equals((long)e.User.Id)).ToDictionary(x => x.UserId, y => y);
                    if (!morphs.ContainsKey((long)e.User.Id))
                    {
                        await e.Channel.SendMessage($"Preparing to instantiate {e.User.Mention}.").ConfigureAwait(false);
                        var questions = new TFQuestions(e);
                        if (ChooseForm.Active.TryAdd((long)e.User.Id, questions))
                        {
                            await questions.StartQuestions().ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await e.Channel.SendMessage($"You've already done that, {e.User.Mention}. Aren't you happy with who you are?").ConfigureAwait(false);
                    }
                });
        }
    }

    internal class TFQuestions
    {
        private readonly CommandEventArgs e;

        public ManualResetEventSlim _answered { get; set; }

        public String current { get; private set; }
        public bool isComplete { get; private set; }

        public static Dictionary<String, String> questions = new Dictionary<String, String> {
            {"height", "How tall are you, in inches?" },
            {"weight", "How much do you weigh, in pounds?" },
            {"hairlength", "How long is your hair, in inches?" },
            {"haircolor", "Which of the following colors most closely matches your hair?\n" +
                "1. Black, 2. Brown, 3. Blond, 4. Auburn, 5. Chestnut, 6. Red, 7. Gray"},
            {"eyecolor", "Which of the following colors most closely matches your eyes?\n" +
                "1. Amber, 2. Blue, 3. Brown, 4. Gray, 5. Green, 6. Hazel, 7. Red, 8. Violet."},
            {"skincolor", "Which of the following colors most closely matches your skin?\n" +
                "1. Umber, 2. Sepia, 3. Ochre, 4. Russet, 5. Terra-Cotta, 6. Gold, 7. Tawny,"+
                " 8. Taupe, 9. Khaki, 10. Fawn, 11. Smaudre." },
            {"musculature", "Which of the following words most closely matches your musculature?\n" +
                ""},
            {"gender", "Which of the following most closely matches your gender?\n" +
                "1. Neutral (they/them), 2. Female (she/her), 3. Male (he/him)."}
        };

        public TFQuestions(CommandEventArgs e)
        {
            this.e = e;
            _answered = new ManualResetEventSlim();
        }

        public async Task StartQuestions()
        {
            try
            {
                NadekoBot.Client.MessageReceived += Answer;

                await e.User.SendMessage($"We're almost ready to create your new body, {e.User.Name}. There are just a few questions to answer first.\n" +
                    "For each question, please respond with a number corresponding to your answer. I'll let you know if anything goes wrong with the process.\n" +
                    "Don't worry if you're not happy with the options now. You can always be changed.").ConfigureAwait(true);

                foreach (KeyValuePair<string, string> q in questions)
                {
                    current = q.Key;
                    await e.User.SendMessage(q.Value).ConfigureAwait(false);
                    _answered.Wait();
                }
            }
            catch (Exception ex) { Console.WriteLine(ex); }

        }

        private async void Answer(object sender, MessageEventArgs e)
        {
            try
            {
                if (!e.Channel.IsPrivate)
                    return;

                int response;
                if (!int.TryParse(e.Message.Text, out response))
                {
                    await e.User.SendMessage("I'm sorry, I couldn't understand that response. It needs to be a number. Could you try again?").ConfigureAwait(false);
                    return;
                }
                _answered.Set();
            }
            catch (Exception ex) { Console.WriteLine(ex); }
        }
    }
}
