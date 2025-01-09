using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
#nullable enable
namespace CopyParametersGadgets
{
    class Class1
    {
        public class MyCacheWall //Кэширующий класс в который добавим все необходимые параметры из элементов Wall для данных вычислений
        {
            private LocationCurve _wallLine; //Добавляем линию основание стены, она понадобится для вычислений
            private XYZ p1 = new XYZ(); //Добавляем первую точку линии основания
            private XYZ p2 = new XYZ(); //Добавляем вторую точку основания линии стены
            public XYZ? pCenter; //Добавляем среднюю точку стены, которую будем вычислять, но не задаем начальное значение

            public MyCacheWall(LocationCurve WallLine) //Конструктор пользовательского класса стены
            {
                _wallLine = WallLine; //Делаем в конструкторе минимум работы - просто передаем нужные нам параметры для вычислений
            }

            public XYZ GetPointCenter(bool cash) //Метод, вычисляющий среднюю точку стены, с ключом определяющим возможность кэширования
            //Улучшим наш неплохой класс добавив возможность кэшировать вычисления средней точки
            {
                p1 = _wallLine.Curve.GetEndPoint(0);
                p2 = _wallLine.Curve.GetEndPoint(1);
                var center = new XYZ((p2.X + p1.X) / 2, (p2.Y + p1.Y) / 2, (p2.Z + p1.Z) / 2); //Тут немножко вспоминаем векторную геометрию за 9 класс школы
                if (cash)
                    pCenter = center;
                return center;
            }

            public double GetLenght(XYZ x) //Метод, вычисляющий расстояние до предложенной ему средней точки другой стены
            {
                if (pCenter is null)
                    return 0.0;
                XYZ vector = new((pCenter.X - x.X), (pCenter.Y - x.Y), (pCenter.Z - x.Z)); //Находим вектор между средней точкой первой стены и второй стены

                return vector.GetLength(); //Находим длину вектора между средними точками двух стен
            }
        }

        string WorkWithWallCashParallel(
            Document doc,
            ICollection<ElementId> selectedIds,
            bool cash,
            bool parallel
        )
        //Ключ cash говорит сооветствующим методам, надо ли использовать кэшированное значение средней точки
        //Ключ parallel определяем в многопоточном режиме работаем или в последовательном
        {
            var minPoints = new List<double>(); //В этом списке будут храниться минимальные расстояния от каждой стены
            var minPointsBag = new ConcurrentBag<double>(); //Это специальная потокобезопасная коллекция, для занесения данных в многопоточном режиме

            DateTime end; //Далее проверим как будет работать наша вычисления в многопоточном режиме
            DateTime start = DateTime.Now; //Засекаем время

            var wallList = selectedIds
                .Select(id => doc.GetElement(id))
                .OfType<Wall>()
                .Select(w => w.Location)
                .OfType<LocationCurve>()
                .Select(c => new MyCacheWall(c))
                .ToList();

            if (parallel) //Если работаем в многопоточном режиме
            {
                System.Threading.Tasks.Parallel.For(
                    0,
                    wallList.Count,
                    i => //Далее будем последовательно у каждого объекта MyWall сравнивать
                    //расстояние от средней точки до средней точки всех остальных объектов (стен). Запускаем задачу в параллельном режиме
                    {
                        List<double> allLenght = new List<double>(); //Это вспомогательный список
                        wallList[i].GetPointCenter(cash); //Находим срединную точку текущего объекта. Больше ее не придется вычислять

                        foreach (MyCacheWall nn in wallList) //проверяем расстояние до каждой срединной точки остальных объектов(стен)
                        {
                            double n = wallList[i].GetLenght(nn.GetPointCenter(cash));
                            if (n != 0) //Исключаем добавление в список текущего объекта
                                allLenght.Add(n); //И записываем все расстояния в этот вспомогательный список
                        }
                        allLenght.Sort(); //Сортируем вспомогательный список

                        minPointsBag.Add(allLenght[0]); //Добавляем наименьшее расстояние в соответствующий потокобезопачный список
                    }
                ); //Заканчиваем задачу
                minPoints.AddRange(minPointsBag); //Размещаем потокобезопасную коллекцию в простой, для удобства работы
            }
            else
            //Если работаем в последовательном режиме

            {
                for (int x = 0; wallList.Count > x; x++) //Далее будем последовательно у каждого объекта MyWall сравнивать
                //расстояние от средней точки до средней точки всех остальных объектов (стен). Запускаем задачу в последовательном режиме
                {
                    List<double> allLenght = new List<double>(); //Это вспомогательный список
                    wallList[x].GetPointCenter(cash); //Находим срединную точку текущего объекта. Больше ее не придется вычислять

                    foreach (MyCacheWall nn in wallList) //проверяем расстояние до каждой срединной точки остальных объектов(стен)
                    {
                        double n = wallList[x].GetLenght(nn.GetPointCenter(cash));
                        if (n != 0) //Исключаем добавление в список текущего объекта
                            allLenght.Add(n); //И записываем все расстояния в этот вспомогательный список
                    }
                    allLenght.Sort(); //Сортируем вспомогательный список

                    minPoints.Add(allLenght[0]); //Добавляем наименьшее расстояние в соответствующий список
                } //Заканчиваем задачу
            }

            minPoints.Sort(); //Сортируем все минимальные расстояния

            double minPoint = minPoints[0]; //Берем самое маленькое расстояние между стенами

            end = DateTime.Now; // Записываем текущее время

            TimeSpan ts = (end - start);

            return ts.TotalMilliseconds.ToString()
                + " миллисекунд. "
                + "\nМин. расстояние между стенами - "
                + (minPoint * 304.8).ToString();
        }
    }
}
