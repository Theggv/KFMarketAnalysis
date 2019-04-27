using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KFMarketAnalysis.Models.Utility
{
    public class RequestConveyorSingleton
    {
        public enum Priority
        {
            Description,
            Item,
            Price,
            Icon
        };

        private static RequestConveyorSingleton instance;

        private Queue<Func<Task<bool>>> highPriority;
        private Queue<Func<Task<bool>>> mediumPriority;
        private Queue<Func<Task<bool>>> lowPriority;

        private Queue<Func<Task<bool>>> withoutPriority;


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


        private RequestConveyorSingleton()
        {
            PriorityConveyor();
            ImagesConveyor();
            
            highPriority = new Queue<Func<Task<bool>>>();
            mediumPriority = new Queue<Func<Task<bool>>>();
            lowPriority = new Queue<Func<Task<bool>>>();
            withoutPriority = new Queue<Func<Task<bool>>>();
        }

        public static RequestConveyorSingleton GetInstance()
        {
            if (instance == null)
            {
                instance = new RequestConveyorSingleton();
            }
            
            return instance;
        }
        
        public void AddAction(Priority priority, Func<Task<bool>> action)
        {
            switch(priority)
            {
                case Priority.Description:
                    highPriority.Enqueue(action);
                    break;
                case Priority.Price:
                    mediumPriority.Enqueue(action);
                    break;
                case Priority.Item:
                    lowPriority.Enqueue(action);
                    break;
                case Priority.Icon:
                    withoutPriority.Enqueue(action);
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
        private async Task<int> Handler(Queue<Func<Task<bool>>> actions, int numTries, bool isNeedDelay = true)
        {
            try
            {
                var action = actions.Peek();

                IAsyncResult asyncResult = action.BeginInvoke(null, null);

                var result = await action.EndInvoke(asyncResult);

                if (!result)
                    numTries++;
                else
                    numTries = 0;
            }
            catch (Exception e)
            {
                numTries++;
            }
            
            if (isNeedDelay)
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
                        numTries = await Handler(withoutPriority, numTries, false);

                        if (numTries > 0 && numTries < 3)
                            continue;
                        else
                            withoutPriority.Dequeue();
                    }
                }
            }); 
        }
    }
}
