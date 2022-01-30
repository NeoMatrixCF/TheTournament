using System;
using System.Collections.ObjectModel;

namespace TournamentOfHeroes
{
    public static class FieldTest
    {
        public static void Start()
        {
            BattleField? field = new();

            Cell beg = field[0],
                end = field[2];
            PathWrite();

            beg = field[5, 5];
            end = field[9, 14];
            PathWrite();

            beg = field[0];
            end = field[^1];
            PathWrite();

            beg = field[0, 14];
            end = field[10, 0];
            PathWrite();

            void PathWrite()
            {
                ReadOnlyCollection<Cell>? path = beg.GetPath(end);
                Console.WriteLine($"Путь от {beg} до {end}:");
                if (path == null)
                    Console.WriteLine("Пути нет.");
                else
                    Console.WriteLine(string.Join(Environment.NewLine, path));
            }
        }

    }
}
