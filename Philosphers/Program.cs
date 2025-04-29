using System.Xml;

namespace philosophers_os
{
    class Fork
    {
        private Mutex m = new Mutex();

        // Устанавливаем мьютекс
        public void Take()
        {
            m.WaitOne();
        }

        // Освобождаем мьютекс
        public void Put()
        {
            m.ReleaseMutex();
        }
    };

    /// <summary>
    /// Класс философа
    /// </summary>
    class Philosopher
    {
        int id; // Идентификатор философа
        Fork leftFork; // Вилка в левой руке
        Fork rightFork; // Вилка в правой руке
        uint lunchesCount; // Количество приемов пищи
        double hungryTime; // Время проведенное без еды
        DateTime hungryTimeStart; // Момент времени, когла началось голодание
        bool end; // Флаг завершения работы приложения
        bool verbalFlag; // Флаг более подробного описания работы
        Random random; // Рандом для генерации случайного времени размышления/питания

        /// <summary>
        /// Размышление
        /// </summary>
        void Think()
        {

            if (this.verbalFlag)
            {
                Console.WriteLine(this.id + " thinking");
            }

            // Думаем
            Thread.Sleep(this.random.Next(0, 100));

            if (this.verbalFlag)
            {
                Console.WriteLine(this.id + " hungry");
            }

            // Начинаем отсчет времени голодания
            this.hungryTimeStart = DateTime.Now;
        }

        /// <summary>
        /// Питание
        /// </summary>
        void Eat()
        {
            // добавляем время голодания к итоговому
            this.hungryTime += DateTime.Now.Subtract(this.hungryTimeStart).TotalMilliseconds;
            if (this.verbalFlag)
            {
                Console.WriteLine(this.id + " eating");
            }

            // Едим
            Thread.Sleep(this.random.Next(0, 100));

            // Увеличиваем количество приемов пищи
            lunchesCount++;
        }

        /// <summary>
        /// Конструктор философа
        /// </summary>
        /// <param name="number">Порядковый номер/идентификатор</param>
        /// <param name="left">Вилка для левой руки</param>
        /// <param name="right">Вилка для правой руки</param>
        /// <param name="verbal">Флаг для вывода в консоль процесса жизни</param>
        public Philosopher(int number, Fork left, Fork right, bool verbal)
        {
            this.id = number;
            this.leftFork = left;
            this.rightFork = right;
            this.lunchesCount = 0;
            this.hungryTime = 0;
            this.verbalFlag = verbal;
            this.end = false;
            this.random = new Random();
        }

        /// <summary>
        /// Запуск работы философа
        /// </summary>
        public void Run()
        {
            // Пока не пора заканчивать
            while (!end)
            {
                // Думаем
                Think();

                // Берем левую вилку
                this.leftFork.Take();
                if (this.verbalFlag)
                {
                    Console.WriteLine(this.id + " took left fork");
                }

                // Берем правую вилку
                this.rightFork.Take();
                if (this.verbalFlag)
                {
                    Console.WriteLine(this.id + " took right fork");
                }

                // Едим
                Eat();

                // Ложим правую вилку
                this.rightFork.Put();
                if (this.verbalFlag)
                {
                    Console.WriteLine(this.id + " put right fork");
                }

                // Ложим левую вилку
                this.leftFork.Put();
                if (this.verbalFlag)
                {
                    Console.WriteLine(this.id + " put left fork");
                }
            }
        }

        /// <summary>
        /// Завершение работы
        /// </summary>
        public void Stop()
        {
            end = true;
        }

        /// <summary>
        /// Вывод информации о жизни философа
        /// </summary>
        public void PrintStats()
        {
            Console.WriteLine("user №\t\tlunches\t\ttime hungry");
            Console.WriteLine(this.id + "\t\t" + this.lunchesCount + "\t\t" + Convert.ToInt32(this.hungryTime));
        }
    };

    class Program
    {
        static void Main(string[] args)
        {

            // Загрузка конфигурации из XML
            Console.WriteLine("Импортируем настройки");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("C:/MyStuff/csharp/philosophers.xml");  // предполагается, что файл лежит рядом с исполняемым

            // Получаем количество пользователей
            int count = int.Parse(xmlDoc.SelectSingleNode("settings/users").InnerText);
            Console.WriteLine("Пользователей: " + count);

            // Получаем настройки вывода информации
            bool verbal = bool.Parse(xmlDoc.SelectSingleNode("settings/verbal_mode").InnerText);
            Console.WriteLine("Подробное описание? " + verbal);

            // Получаем длительность работы
            int duration = int.Parse(xmlDoc.SelectSingleNode("settings/timer").InnerText);
            Console.WriteLine("Длительность работы: " + duration);

            // Добавляем вилки
            Fork[] forks = new Fork[count];
            for (int i = 0; i < count; i++) forks[i] = new Fork();

            // Добавляем философов
            Philosopher[] phils = new Philosopher[count];
            for (int i = 0; i < count; i++)
            {
                phils[i] = new Philosopher(i + 1, forks[(i + 1) % count], forks[i], verbal);
            }

            // Инициализируем запуски
            Thread[] runners = new Thread[count];
            for (int i = 0; i < count; i++) runners[i] = new Thread(phils[i].Run);
            for (int i = 0; i < count; i++) runners[i].Start();

            // В конце выполнения
            Thread.Sleep(duration);

            // Останавливаем и блокируем потоки
            for (int i = 0; i < count; i++) phils[i].Stop();
            for (int i = 0; i < count; i++) runners[i].Join();

            for (int i = 0; i < count; i++) phils[i].PrintStats();
        }
    }
}