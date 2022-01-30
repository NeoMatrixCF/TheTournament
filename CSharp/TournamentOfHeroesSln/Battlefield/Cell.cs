using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TournamentOfHeroes
{
    /// <summary>Ячейка игрового поля.</summary>
    public class Cell
    {
        /// <summary>Сквозной индекс.</summary>
        public int Index { get; }

        /// <summary>Индекс строки.</summary>
        public int Row { get; }

        /// <summary>Индекс колонки.</summary>
        public int Column { get; }

        /// <summary>Соседние ячейки.</summary>
        public IReadOnlyList<Cell> Neighbors { get; private set; } = new List<Cell>();

        /// <summary>Заморозка соседей.</summary>
        public void FreezeNeighbors()
        {
            if (Neighbors is List<Cell> list)
            {
                Neighbors = Array.AsReadOnly(list.ToArray());
            }
        }

        /// <summary>Добавление соседа.</summary>
        /// <param name="neighbor">Соседняя ячейка.</param>
        public void AddNeighbors(Cell neighbor)
        {
            ((List<Cell>)Neighbors).Add(neighbor);
        }

        /// <summary>Добавление соседей.</summary>
        /// <param name="neighbors">Соседнии ячейки.</param>
        public void AddRangeNeighbors(IEnumerable<Cell> neighbors)
        {
            ((List<Cell>)Neighbors).AddRange(neighbors);
        }

        /// <summary>Инициализирует экземпляр ячейки.</summary>
        /// <param name="index">Сквозной индекс.</param>
        /// <param name="row">Индекс строки.</param>
        /// <param name="column">Индекс колонки.</param>
        public Cell(int index, int row, int column)
        {
            Index = index;
            Row = row;
            Column = column;
        }

        /// <summary>Свойство для типа ланшафта.</summary>
        public object Terrain { get; set; }

        /// <summary>Свойство для контента (юнита).</summary>
        public object Content { get; set; }

        /// <summary>Получение соседей текущей ячейки до заданной глубины.</summary>
        /// <param name="deep">Глубина. Если меньше 1, то принимается равной 1.</param>
        /// <returns>Возвращает словарь, где ключ - глубина залегания соседей,
        /// а значение - коллекция соседей с этой глубины.</returns>
        public ReadOnlyDictionary<int, ReadOnlyCollection<Cell>> GetNeighbors(int deep = 1)
            => GetNeighbors(this, deep);

        /// <summary>Получение соседей ячейки до заданной глубины.</summary>
        /// <param name="cell">Ячейка чьих соседей нужно получить.</param>
        /// <param name="deep">Глубина. Если меньше 1, то принимается равной 1.</param>
        /// <returns>Возвращает словарь, где ключ - глубина залегания соседей,
        /// а значение - коллекция соседей с этой глубины.</returns>
        public static ReadOnlyDictionary<int, ReadOnlyCollection<Cell>> GetNeighbors(Cell cell, int deep = 1)
        {
            if (deep < 1)
            {
                deep = 1;
            }

            int level = 0;
            HashSet<Cell> cells = new() { cell };
            Queue<(int deep, Cell cell)> queue = new(new[] { (level, cell) });
            Dictionary<int, ReadOnlyCollection<Cell>> neighbors = new();
            List<Cell> neighborsLevel = new(cell.Neighbors.Count * level);
            while (queue.Count > 0 && level <= deep)
            {
                var curr = queue.Dequeue();
                if (curr.deep >= level)
                {
                    if (neighborsLevel.Count > 0)
                    {
                        neighbors.Add(level, Array.AsReadOnly(neighborsLevel.ToArray()));
                    }
                    neighbors.Clear();
                    level++;
                }
                if (cells.Add(curr.cell))
                {
                    neighborsLevel.Add(curr.cell);
                }
                foreach (var cll in curr.cell.Neighbors)
                {
                    queue.Enqueue((level + 1, cll));
                }
            }

            return new ReadOnlyDictionary<int, ReadOnlyCollection<Cell>>(neighbors);
        }

        /// <summary>Получение пути от текущей ячейки до другой.</summary>
        /// <param name="end">Ячейка на которой заканчивается путь.</param>
        /// <returns>Коллекцию ячеек, начинающуюся с текущей и закачивающейся <paramref name="end"/>, если есть путь между ними.<br/>
        /// Иначе <see langword="null"/>.</returns>
        public ReadOnlyCollection<Cell> GetPath(Cell end)
            => GetPath(this, end);


        /// <summary>Получение пути от одной ячейки до другой.</summary>
        /// <param name="begin">Ячейка с которой начинается путь.</param>
        /// <param name="end">Ячейка на которой заканчивается путь.</param>
        /// <returns>Коллекцию ячеек начинающуюся с <paramref name="begin"/> и закачивающейся <paramref name="end"/>? если есть путь между ними.<br/>
        /// Иначе <see langword="null"/>.</returns>
        public static ReadOnlyCollection<Cell> GetPath(Cell begin, Cell end)
        {
            HashSet<Cell> cells = new() { begin };
            Queue<(IEnumerable<Cell> path, Cell cell)> queue = new(new[] { (Enumerable.Repeat(begin, 1), begin) });
            while (queue.Count > 0)
            {
                var curr = queue.Dequeue();
                if (curr.cell == end)
                {
                    return Array.AsReadOnly(curr.path.ToArray());
                }
                foreach (var cll in curr.cell.Neighbors)
                {
                    if (cells.Add(cll))
                    {
                        queue.Enqueue((curr.path.Append(cll), cll));
                    }
                }
            }

            return null;
        }

        public override string ToString()
            => $"{Index} - ({Row}, {Column})";
    }
}
