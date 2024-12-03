using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ReMount
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RepareMountService remount = new RepareMountService();

            remount.Init();

            remount.Work();
        }
    }

    class RepareMountService
    {
        private int _penaltyMissingPart = 100;
        private int _penaltyWrongPart = 300;
        private int _money = 2000;
        private int _workerPayment = 300;
        private string _nameTeacher = "Наставник: ";
        private string _indent = "\n";
        private Storage _storage = new Storage();
        private Mount _client;
        private MountCreator _clientGenerator = new MountCreator();
        private PartCreator _partCreator = new PartCreator();

        public void Init()
        {
            Console.WriteLine($"{_nameTeacher}Добро пожаловать, стажёр, в нашу мастерскую маунтов!" +
                $"{_indent}Как ты уже знаешь, маунт - это ездовое животное." +
                $"{_indent}В наше тяжелое время они быстро приходят в негодность." +
                $"{_indent}Здесь их можно привести в порядок, чтобы восстановить их полезность." +
                $"{_indent}Почини несколько чужих маунтов, чтобы разобраться во всём. Заодно заработаешь денег на собственного маунта." +
                $"{_indent}Глянь сколько деталей ты видишь на складе?");

            int storageSize = UserUtils.GetPositiveIntUserInput();

            for (int i = 0; i < storageSize; i++)
            {
                _storage.AddPart(_partCreator.CreateRandomPart());
            }

            Console.Clear();
            Console.WriteLine($"{_nameTeacher}В кассу я положил немного монет, чтобы ты мог работать." +
                $"{_indent}Помни для ОПРЕДЕЛЁННОЙ части маунта есть СООТВЕТСТВУЮЩАЯ деталь!" +
                $"{_indent}Также маунты РАЗЛИЧАЮТСЯ ПО СРЕДЕ в которой перемещаются." +
                $"{_indent}Если НЕТ нужной детали, мы заплатим ШТРАФ {_penaltyMissingPart}! Такова политика предприятия." +
                $"{_indent}Но можно РИСКНУТЬ, если не хочешь платить штраф, и поставить деталь для ДРУГОЙ СРЕДЫ эксплуатации." +
                $"{_indent}Правда если она не подойдёт и это ЗАМЕТИТ клиент - ШТРАФ будет больше - {_penaltyWrongPart}. А это фифти-фифти, как говорится." +
                $"{_indent}Так что будь внимательнее, и удачи, малец!");
            Console.ReadKey();
        }

        public void Work()
        {
            bool isWorking = true;

            while (isWorking)
            {
                Console.Clear();

                ShowInfo();

                Console.WriteLine(_nameTeacher + "У нас новый клиент!");

                _client = _clientGenerator.CreateRandomMount();

                List<Part> brokenParts = _client.ReturnBrokenParts();

                Console.WriteLine($"У него {brokenParts.Count} штуки поломатого:");

                foreach (Part part in brokenParts)
                {
                    part.ShowInfo();

                    if (TryFindPart(part.Belonging, out Part newPart))
                    {
                        ReparePart(newPart);

                        _money += CalculateFinalPrice(newPart);
                    }
                    else
                    {
                        Console.WriteLine("...ничего...Тогда штраф -" + _penaltyMissingPart);
                        _money -= _penaltyMissingPart;
                    }
                }

                ConsoleKey keyExit = ConsoleKey.Escape;

                Console.WriteLine($"Нажмите любую клавишу для следущего клиента, или {keyExit} для выхода.");

                if (Console.ReadKey(true).Key == keyExit || _money <= 0 || _storage.ReturnPartsCount <= 0)
                {
                    isWorking = false;
                }
            }

            SayFinalSpeech();
        }

        private void ShowInfo()
        {
            Console.WriteLine($"Количество деталей на складе: {_storage.ReturnPartsCount}" +
                $"{_indent}Количество денег: {_money}");
        }

        private void ReparePart(Part newPart)
        {
            _client.SetNewPart(newPart);
            _storage.RemovePart(newPart);
        }

        private void SayFinalSpeech()
        {
            if (_money <= 0)
            {
                Console.WriteLine($"{_nameTeacher}Денежки промотал, сынок.");
            }

            if (_storage.ReturnPartsCount <= 0)
            {
                Console.WriteLine($"{_nameTeacher}Детальки то на складе не бесконечные, малец.");
            }

            Console.WriteLine($"{_nameTeacher}Надеюсь тебе было понятно, стажёр, как мы тут каждый день крутимся...Удачи, сынок!");
        }

        private bool TryFindPart(MountParts belonging, out Part newPart)
        {
            bool isFinding = false;
            newPart = null;

            Console.WriteLine("На складе вместо сломаной детали есть:");

            List<Part> findingParts = _storage.ReturnPartsByBelonging(belonging);

            if (findingParts.Count > 0)
            {
                for (int i = 0; i < findingParts.Count; i++)
                {
                    Console.Write($"{i + 1} найденная деталь. За {findingParts[i].Price + _workerPayment} денег. ");
                    findingParts[i].ShowInfo();
                }

                Console.WriteLine("Какую будем устанавливать, введите номер? введёшь цифру ноль - не вставишь ничего");

                int index = UserUtils.GetPositiveIntUserInput(findingParts.Count) - 1;

                if (index >= 0)
                {
                    newPart = findingParts[index];
                    isFinding = true;
                }
                else
                {
                    Console.WriteLine("Мы решили НИЧЕГО НЕ СТАВИТЬ");
                }
            }

            return isFinding;
        }

        private int CalculateFinalPrice(Part part)
        {
            bool isClientNotice = UserUtils.GetRandomBool();
            int finalPrice = 0;

            if (part.IsWorking)
            {
                Console.WriteLine("Мы установили рабочую деталь.");

                if (part.ApplicationEnvironment == _client.TravelEnvironment)
                {
                    finalPrice = part.Price + _workerPayment;
                    Console.WriteLine("И она подходит по среде эксплуатации. Успех! Заработок +" + finalPrice);
                }
                else
                {
                    Console.WriteLine("Но она не подходит по среде эксплуатации. ");

                    if (isClientNotice == false)
                    {
                        finalPrice = part.Price + _workerPayment;
                        Console.WriteLine("Клиент этого не заметил, так что денежки мы получили...+" + finalPrice);
                    }
                    else
                    {
                        Console.WriteLine("Клиент заметил наш косяк...Деталь протеряли... и штраф -" + _penaltyWrongPart);
                        finalPrice -= _penaltyWrongPart;
                    }
                }
            }
            else
            {
                Console.WriteLine("Мы установили бракованную деталь. Редкий случай блин. Штраф -" + _penaltyWrongPart);
                finalPrice -= _penaltyWrongPart;
            }

            return finalPrice;
        }
    }

    class Storage
    {
        private List<Part> _parts = new List<Part>();

        public int ReturnPartsCount => _parts.Count;

        public void AddPart(Part part)
        {
            _parts.Add(part);
        }

        public void RemovePart(Part part)
        {
            _parts.Remove(part);
        }

        public List<Part> ReturnPartsByBelonging(MountParts requiredMountPart)
        {
            List<Part> requiredParts = new List<Part>();

            foreach (Part part in _parts)
            {
                if (part.Belonging == requiredMountPart)
                {
                    requiredParts.Add(part);
                }
            }

            return requiredParts.ToList();
        }
    }

    class Mount
    {
        private List<Part> _parts = new List<Part>();
        private PartCreator _partCreator = new PartCreator();
        private List<MountParts> _possibleParts = Enum.GetValues(typeof(MountParts)).Cast<MountParts>().ToList();

        public Mount(ApplicationEnvironments travelEnvironment)
        {
            TravelEnvironment = travelEnvironment;
            CreateParts();
            BreakPart();
        }

        public ApplicationEnvironments TravelEnvironment { get; private set; }

        public void SetNewPart(Part newPart)
        {
            for (int i = 0; i < _parts.Count; i++)
            {
                if (_parts[i].Belonging == newPart.Belonging)
                {
                    _parts[i] = newPart;
                }
            }
        }

        public List<Part> ReturnBrokenParts()
        {
            List<Part> parts = new List<Part>();

            foreach (Part part in _parts)
            {
                if (part.IsWorking == false)
                {
                    parts.Add(part);
                }
            }

            return parts.ToList();
        }

        private void BreakPart()
        {
            bool hasBrokenPart = false;

            foreach (Part part in _parts)
            {
                if (part.IsWorking == false)
                {
                    hasBrokenPart = true;

                    break;
                }
            }

            if (hasBrokenPart == false)
            {
                _parts[UserUtils.GetRandomPositiveNumber(_parts.Count)].Break();

            }
        }

        private void CreateParts()
        {
            foreach (MountParts mountPart in _possibleParts)
            {
                _parts.Add(_partCreator.CreatePart(TravelEnvironment, mountPart));
            }
        }
    }

    class MountCreator
    {
        private List<ApplicationEnvironments> _applicationEnvironments = Enum.GetValues(typeof(ApplicationEnvironments)).Cast<ApplicationEnvironments>().ToList();

        public Mount CreateRandomMount()
        {
            return new Mount(_applicationEnvironments[UserUtils.GetRandomPositiveNumber(_applicationEnvironments.Count)]);
        }
    }

    class Part
    {
        public Part(ApplicationEnvironments applicationEnvironments, MountParts belonging, int price, bool isWorking)
        {
            ApplicationEnvironment = applicationEnvironments;
            Belonging = belonging;
            Price = price;
            IsWorking = isWorking;
        }

        public ApplicationEnvironments ApplicationEnvironment { get; private set; }
        public MountParts Belonging { get; private set; }
        public int Price { get; private set; }
        public bool IsWorking { get; private set; }

        public void Break()
        {
            IsWorking = false;
        }

        public void ShowInfo()
        {
            string separator = " | ";

            Console.WriteLine($"Среда применения - {ApplicationEnvironment}{separator}Часть маунта - {Belonging}{separator}");
        }
    }

    class PartCreator
    {
        private int _defectChance = 10;
        private int _maxChance = 100;
        private List<MountParts> _mountParts = Enum.GetValues(typeof(MountParts)).Cast<MountParts>().ToList();
        private List<ApplicationEnvironments> _applicationEnvironments = Enum.GetValues(typeof(ApplicationEnvironments)).Cast<ApplicationEnvironments>().ToList();

        public Part CreateRandomPart()
        {
            return new Part(_applicationEnvironments[UserUtils.GetRandomPositiveNumber(_applicationEnvironments.Count)], _mountParts[UserUtils.GetRandomPositiveNumber(_mountParts.Count)], GeneratePrice(), UserUtils.GetRandomPositiveNumber(_maxChance) > _defectChance);
        }

        public Part CreatePart(ApplicationEnvironments applicationEnvironment, MountParts part)
        {
            return new Part(applicationEnvironment, part, GeneratePrice(), UserUtils.GetRandomPositiveNumber(_maxChance) > _defectChance);
        }

        private int GeneratePrice()
        {
            int minPrice = 100;
            int maxPrice = 500;

            return UserUtils.GetRandomNumber(minPrice, maxPrice);
        }
    }

    enum ApplicationEnvironments
    {
        [Description("Земля")]
        Earth,
        [Description("Вода")]
        Water,
        [Description("Воздух")]
        Air
    }

    static class UserUtils
    {
        private static Random s_random = new Random();

        public static bool GetRandomBool()
        {
            bool[] boolValues = { true, false };

            return boolValues[s_random.Next(boolValues.Length)];
        }

        public static int GetRandomPositiveNumber(int maxValue)
        {
            return s_random.Next(maxValue);
        }

        public static int GetRandomNumber(int minValue, int maxValue)
        {
            return s_random.Next(minValue, maxValue);
        }

        public static int GetPositiveIntUserInput()
        {
            int userInput;

            do
            {
                Console.WriteLine("Ваш выбор: ");
            }
            while (int.TryParse(Console.ReadLine(), out userInput) == false || userInput < 0);

            return userInput;
        }

        public static int GetPositiveIntUserInput(int maxValue)
        {
            int userInput;

            do
            {
                Console.WriteLine("Ваш выбор: ");
            }
            while (int.TryParse(Console.ReadLine(), out userInput) == false || userInput < 0 || userInput > maxValue);

            return userInput;
        }
    }

    enum MountParts
    {
        [Description("Голова")]
        Head,
        [Description("Туловище")]
        Body,
        [Description("Конечность")]
        Limb,
        [Description("Седло")]
        Saddle
    }
}