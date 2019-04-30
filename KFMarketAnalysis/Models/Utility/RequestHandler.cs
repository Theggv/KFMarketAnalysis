using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KFMarketAnalysis.Models.Utility
{
    public class RequestHandler
    {
        public enum Priority
        {
            High,
            Medium,
            Low,
            WithoutDelay
        };

        private static RequestHandler instance;

        private Queue<Request> highPriority;
        private Queue<Request> mediumPriority;
        private Queue<Request> lowPriority;

        private Queue<Request> withoutPriority;


        public static int Delay
        {
            get
            {
                const int minTime = 750;
                const int standartTime = 4000;

                if (!ProxySingleton.GetInstance().CanUse)
                    return standartTime;

                var time = (standartTime + minTime) / ProxySingleton.GetInstance().Count;

                return time > minTime ? time : minTime;
            }
        }


        private RequestHandler()
        {
            PriorityConveyor();
            ImagesConveyor();
            
            highPriority = new Queue<Request>();
            mediumPriority = new Queue<Request>();
            lowPriority = new Queue<Request>();
            withoutPriority = new Queue<Request>();
        }

        public static RequestHandler GetInstance()
        {
            if (instance == null)
            {
                instance = new RequestHandler();
            }
            
            return instance;
        }
        
        public void AddAction(Priority priority, Func<Task<bool>> action, bool isNeedDelay = true)
        {
            switch(priority)
            {
                case Priority.High:
                    highPriority.Enqueue(new Request(action, isNeedDelay));
                    break;
                case Priority.Medium:
                    mediumPriority.Enqueue(new Request(action, isNeedDelay));
                    break;
                case Priority.Low:
                    lowPriority.Enqueue(new Request(action, isNeedDelay));
                    break;
                case Priority.WithoutDelay:
                    withoutPriority.Enqueue(new Request(action, false));
                    break;
            }
        }

        private void PriorityConveyor()
        {
            int numTries = 0;

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(100);

                    if(highPriority.Count > 0)
                    {
                        numTries = await Handler(highPriority, numTries);

                        if (numTries > 0 && numTries < 5)
                            continue;
                        else
                            highPriority.Dequeue();
                    }
                    else if (mediumPriority.Count > 0)
                    {
                        numTries = await Handler(mediumPriority, numTries);

                        if (numTries > 0 && numTries < 4)
                            continue;
                        else
                            mediumPriority.Dequeue();
                    }
                    else if (lowPriority.Count > 0)
                    {
                        numTries = await Handler(lowPriority, numTries);

                        if (numTries > 0 && numTries < 3)
                            continue;
                        else
                            lowPriority.Dequeue();
                    }
                }
            });
        }

        /// <summary>
        /// Обработчик Func делегатов
        /// </summary>
        /// <param name="actions">Очередь функций для выполнения</param>
        /// <param name="numTries">Количество совершённых попыток</param>
        /// <param name="isNeedDelay">Нужен ли анти-спам</param>
        /// <returns>Количество совершённых попыток</returns>
        private async Task<int> Handler(Queue<Request> actions, int numTries)
        {
            try
            {
                var request = actions.Peek();

                IAsyncResult asyncResult = request.Action.BeginInvoke(null, null);

                var result = await request.Action.EndInvoke(asyncResult);

                if (!result)
                    numTries++;
                else
                    numTries = 0;
            }
            catch (Exception e)
            {
                numTries++;
            }
            
            if (actions.Peek().IsNeedDelay)
                await Task.Delay(Delay);

            return numTries;
        }

        private void ImagesConveyor()
        {
            int numTries = 0;

            Task.Run(async () =>
            {
                while (true)
                {
                    // await Task.Delay(50);

                    if (withoutPriority.Count > 0)
                    {
                        numTries = await Handler(withoutPriority, numTries);

                        if (numTries > 0 && numTries < 3)
                            continue;
                        else
                            withoutPriority.Dequeue();
                    }
                }
            }); 
        }

        sealed class Request
        {
            public Func<Task<bool>> Action { get; set; }

            public bool IsNeedDelay { get; set; }

            public Request(Func<Task<bool>> action, bool isNeedDelay)
            {
                Action = action;
                IsNeedDelay = isNeedDelay;
            }
        }
    }
}
