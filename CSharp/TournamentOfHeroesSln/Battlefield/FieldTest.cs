using System;
using System.Linq;

namespace TournamentOfHeroes
{
    public static class FieldTest
    {
        public static void Start()
        {
            var field = new BattleField();

            var path = field[0].GetPath(field[2]);

            Console.WriteLine(string.Join(Environment.NewLine, path.Select(c => $"{c.Index} - ({c.Row}, {c.Column})")));


            path = field[5, 5].GetPath(field[9, 14]);
            Console.WriteLine();
            Console.WriteLine(string.Join(Environment.NewLine, path));

            path = field[0].GetPath(field[field.Count - 1]);
            Console.WriteLine();
            Console.WriteLine(string.Join(Environment.NewLine, path));

            path = field[0, 14].GetPath(field[10, 0]);
            Console.WriteLine();
            Console.WriteLine(string.Join(Environment.NewLine, path));
        }
    }
}
