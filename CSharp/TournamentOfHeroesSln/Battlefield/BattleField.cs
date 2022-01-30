using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TournamentOfHeroes
{
    //TODO: Нужно картинку поля предоставить через ГитХаб.

    /// <summary>Поле битвы.</summary>
    /// <remarks>Пока реализована только постоянная конфигурация:
    /// <see href="https://www.cyberforum.ru/attachments/1266235d1625492691"/>.</remarks>
    public class BattleField : IEnumerable<Cell>, IReadOnlyList<Cell>, ICollection
    {


        /// <summary>Получение сквозного индекса по индексам строки и колонки.</summary>
        /// <param name="row">Индекс строки.</param>
        /// <param name="column">Индекс колонки.</param>
        /// <returns>Сквозной индекс.</returns>
        public static int GetIndex(int row, int column)
        {
            int index = row * 15;
            index += row / 2;
            index += column;
            return index;
        }

        /// <summary>Получение индексов строки и колонки по сквозному индексу.</summary>
        /// <param name="index">Сквозной индекс.</param>
        /// <returns>Кортеж с индексами строки и колонки.</returns>
        public static (int row, int column) GetIndex2D(int index)
        {
            int pair = index / 31; // 15+16

            int column = index - pair * 31;
            bool evenRow = column > 14;
            if (evenRow)
                column -= 15;

            int row = pair * 2 + (evenRow ? 1 : 0);

            return (row, column);
        }

        /// <summary>Ячейки поле битвы.</summary>
        public ReadOnlyCollection<ReadOnlyCollection<Cell>> Cells { get; }

        /// <summary>Инициализирует экземпляр поля битвы.</summary>
        public BattleField()
        {
            // Создание матрицы ячеек
            ReadOnlyCollection<Cell>[] cells = new ReadOnlyCollection<Cell>[Matrix.Count];
            for (int i = 0; i < cells.Length; i++)
            {
                Cell[] row = new Cell[Matrix[i].Count];
                for (int j = 0; j < row.Length; j++)
                {
                    row[j] = new Cell(GetIndex(i, j), i, j);
                }
                cells[i] = Array.AsReadOnly(row);
            }

            // Запись соседей в ячейки
            for (int i = 0; i < cells.Length; i++)
            {
                for (int j = 0; j < cells[i].Count; j++)
                {
                    cells[i][j].AddRangeNeighbors(Matrix[i][j].Select(tp => cells[tp.row][tp.column]));
                    cells[i][j].FreezeNeighbors();
                }
            }

            Cells = Array.AsReadOnly(cells);

            Count = cells.Select(row => row.Count).Sum();
        }

        /// <summary>Возвращает ячейку по сквозному индексу.</summary>
        /// <param name="index">Сквозной индекс.</param>
        /// <returns>Ячейка этого поля по указанному индексу.</returns>
        public Cell this[int index]
        {
            get
            {
                var (row, column) = GetIndex2D(index);
                return Cells[row][column];
            }
        }

        /// <summary>Возвращает ячейку по индексам строки и колонки.</summary>
        /// <param name="row">Индекс строки.</param>
        /// <param name="column">Индекс колонки.</param>
        /// <returns>Ячейка этого поля по указанным индексам.</returns>
        public Cell this[int row, int column] => Cells[row][column];

        /// <summary>Матрицы связности поля битвы.</summary>
        public static ReadOnlyCollection<ReadOnlyCollection<ReadOnlyCollection<(int index, int row, int column)>>> Matrix { get; }

        /// <summary>Общее количество ячеек.</summary>
        public int Count { get; }

        /// <inheritdoc cref="ICollection.IsSynchronized"/>
        public bool IsSynchronized => true;

        /// <inheritdoc cref="ICollection.SyncRoot"/>
        public object SyncRoot => this;

        /// <summary>Статический конструтор инициализирующий <see cref="Matrix"/>.</summary>
        static BattleField()
        {
            ReadOnlyCollection<ReadOnlyCollection<(int index, int row, int column)>>[] matrixRO = new ReadOnlyCollection<ReadOnlyCollection<(int index, int row, int column)>>[11];

            var matrix = GetBattleFieldMatrix();
            for (int i = 0; i < 11; i++)
            {
                ReadOnlyCollection<(int index, int row, int column)>[] row = new ReadOnlyCollection<(int index, int row, int column)>[matrix[i].Length];
                for (int j = 0; j < row.Length; j++)
                {
                    row[j] = Array.AsReadOnly(matrix[i][j]);
                }
                matrixRO[i] = Array.AsReadOnly(row);
            }

            Matrix = Array.AsReadOnly(matrixRO);
        }


        /// <summary>Вспомогательный метод создающий мутабельную матрицу смежности.</summary>
        /// <returns>Мутабельную матрицу смежности.</returns>
        /// <remarks>В статическом конструторе эта матрица преобразуется в иммутабельную.</remarks>
        private static (int index, int row, int column)[][][] GetBattleFieldMatrix()
        {
            (int index, int row, int column)[][][] matrix = new (int index, int row, int column)[11][][];

            // Заполнение первой строки
            (int index, int row, int column)[][] row = new (int index, int row, int column)[15][];
            matrix[0] = row;
            // Первый элемент 
            row[0] = new (int index, int row, int column)[] { GetTupleIndex(0, 1), GetTupleIndex(1, 0), GetTupleIndex(1, 1) };
            // Последний элемент 
            row[14] = new[] { GetTupleIndex(0, 13), GetTupleIndex(1, 14), GetTupleIndex(1, 15) };
            // Средние элементы
            for (int i = 1; i < 14; i++)
            {
                row[i] = new[] { GetTupleIndex(0, i - 1), GetTupleIndex(1, i), GetTupleIndex(1, i + 1), GetTupleIndex(0, i + 1) };
            }

            // Заполнение последней строки
            row = new (int index, int row, int column)[15][];
            matrix[10] = row;
            // Первый элемент 
            row[0] = new[] { GetTupleIndex(10, 1), GetTupleIndex(9, 0), GetTupleIndex(9, 1) };
            // Последний элемент 
            row[14] = new[] { GetTupleIndex(10, 13), GetTupleIndex(9, 14), GetTupleIndex(9, 15) };
            // Средние элементы
            for (int i = 1; i < 14; i++)
            {
                row[i] = new[] { GetTupleIndex(10, i - 1), GetTupleIndex(9, i), GetTupleIndex(9, i + 1), GetTupleIndex(10, i + 1) };
            }

            // Заполнение чётных (по индексу) строк
            for (int r = 2; r < 10; r += 2)
            {
                row = new (int index, int row, int column)[15][];
                matrix[r] = row;
                // Первый элемент 
                row[0] = new[] { GetTupleIndex(r - 1, 0), GetTupleIndex(r - 1, 1), GetTupleIndex(r, 1), GetTupleIndex(r + 1, 0), GetTupleIndex(r + 1, 1) };
                // Последний элемент 
                row[14] = new[] { GetTupleIndex(r - 1, 14), GetTupleIndex(r - 1, 15), GetTupleIndex(r, 13), GetTupleIndex(r + 1, 14), GetTupleIndex(r + 1, 15) };
                // Средние элементы
                for (int i = 1; i < 14; i++)
                {
                    row[i] = new[] { GetTupleIndex(r, i - 1), GetTupleIndex(r - 1, i), GetTupleIndex(r - 1, i + 1), GetTupleIndex(r, i + 1), GetTupleIndex(r + 1, i), GetTupleIndex(r + 1, i + 1) };
                }
            }

            // Заполнение нечётных (по индексу) строк
            for (int r = 1; r < 10; r += 2)
            {
                row = new (int index, int row, int column)[16][];
                matrix[r] = row;
                // Первый элемент 
                row[0] = new[] { GetTupleIndex(r - 1, 0), GetTupleIndex(r, 1), GetTupleIndex(r + 1, 0) };
                // Последний элемент 
                row[15] = new[] { GetTupleIndex(r, 14), GetTupleIndex(r - 1, 14), GetTupleIndex(r + 1, 14) };
                // Средние элементы
                for (int i = 1; i < 15; i++)
                {
                    row[i] = new[] { GetTupleIndex(r, i - 1), GetTupleIndex(r - 1, i - 1), GetTupleIndex(r - 1, i), GetTupleIndex(r, i + 1), GetTupleIndex(r + 1, i - 1), GetTupleIndex(r + 1, i) };
                }
            }

            return matrix;

            (int index, int row, int column) GetTupleIndex(int row, int column) => (GetIndex(row, column), row, column);
        }

        /// <inheritdoc cref="IEnumerable{Cell}.GetEnumerator"/>
        public IEnumerator<Cell> GetEnumerator()
        {
            foreach (var row in Cells)
            {
                foreach (var cell in row)
                {
                    yield return cell;
                }
            }
        }

        /// <inheritdoc cref="IEnumerable.GetEnumerator"/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Не реализован.</summary>
        /// <exception cref="NotImplementedException"/>
        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }
    }
}
