//bombaclat
using System.Text.RegularExpressions;

Console.OutputEncoding = System.Text.Encoding.UTF8;

//nastavení hry
int VelikostPole = 10;  //DODĚLAT ŽE NEFUGUJE FORMÁTOVÁNÍ TABULKY KDYŽ JE TABULKA VĚTŠÍ NEŽ 10
int HlavickaXPad = VelikostPole.ToString().Length + 1;
int PocetMin = 20;

// Key = X; Value = Y
List<KeyValuePair<int, int>> TahyHrace = new();
List<KeyValuePair<int, int>> LokaceVlajek = new();
List<KeyValuePair<int, int>> LokaceMin = GenMines(PocetMin, VelikostPole);

bool repeatGame = true;
DateTime timer = DateTime.Now;



//game loop
while(repeatGame)
{
    Console.Clear();
    //info ke hře
    Console.WriteLine($"Čas hry: {getFormatedTimer()}");
    Console.WriteLine($"Počet min: {PocetMin} | Zavlajkovaných polí: {LokaceVlajek.Count}\n");

    DrawGame(VelikostPole);
    Console.WriteLine("\nZadej další souřadnice (formát: X Y[F]) (pro pomoc s příkazy zadej: help):");
    while (true)
    {
        if (!MakeTurn(Console.ReadLine())) Console.WriteLine("Neplatné souřadnice! Zkus to znovu a lépe >:(");
        else break;
    }

    //VÝHRA HRY
    if (Math.Pow(VelikostPole, 2) == TahyHrace.Count + PocetMin) GameWin();
}



//vykreslý herní pole
void DrawGame(int velikostPole, KeyValuePair<int, int>? naslaplaMina = null)
{
    //10x10 pole
    for (int y = -1; y <= velikostPole; y++)
    {
        if (y == -1) Console.Write("Y X".PadLeft(HlavickaXPad)+"| ");
        else if (y == 0) Console.Write("‾".PadRight(HlavickaXPad)+"+-");
        else Console.Write($"{y}".PadRight(HlavickaXPad)+"| ");

        for (int x = 1; x <= velikostPole; x++)
        {
            if (y == -1) Console.Write($"{x} ");
            else if (y == 0) Console.Write(string.Concat(Enumerable.Repeat("-", x.ToString().Length + 1)));
            else
            {
                int minyVOkoli = CountMinesAroundCords(new(x, y));
                if (TahyHrace.Contains(new(x, y))) Console.Write($"{(minyVOkoli == 0 ? " " : minyVOkoli)} ");
                else if (naslaplaMina != null)
                {
                    if (naslaplaMina.Equals(new KeyValuePair<int, int>(x, y))) Console.Write("X ");
                    else if (LokaceMin.Contains(new(x, y)))
                    {
                        if (LokaceVlajek.Contains(new(x, y))) Console.Write("⚑ ");
                        else Console.Write("x ");
                    }
                    else Console.Write("  ");
                }
                else if (LokaceVlajek.Contains(new(x, y))) Console.Write("⚑ ");
                else Console.Write("• ");
            }
        }
        Console.WriteLine();
    }
}

//turn based funkce
bool MakeTurn(string? input)
{
    bool flagTurn = false;
    var inputCords = Regex.Matches(input!, @"(\d{1,}[Ff]?)");

    if (inputCords.Count == 2)
    {
        //zpracování inputu pro případ, že hráč chce ovlajkovat pole
        List<int>? tmpCords = new();
        for (int i = 0; i < 2; i++)
        {
            if (!int.TryParse(inputCords[i].Value, out int cord))
            {
                var inputCordsStr = inputCords[i].ToString().ToLower();
                if (inputCordsStr.Contains('f'))
                {
                    if (!int.TryParse(inputCordsStr.Remove(inputCordsStr.IndexOf('f'), 1), out cord)) return false;
                    flagTurn = true;
                }
                else return false;
            }
            //kontrola, jestli zadané souřadnice vůbec existuje v hracím poli
            if (cord < 1 || cord > VelikostPole) return false;
            tmpCords.Add(cord);
        }

        KeyValuePair<int, int> newCords = new(tmpCords[0], tmpCords[1]);
        tmpCords = null;


        //pokud je tah vlajkovací
        if (flagTurn && !TahyHrace.Contains(newCords))
        {
            if (LokaceVlajek.Contains(newCords)) LokaceVlajek.Remove(newCords);
            else if (LokaceVlajek.Count < PocetMin) LokaceVlajek.Add(newCords);
            return true;
        }
        else if (LokaceVlajek.Contains(newCords)) return true; //pokud se snaží odkrýt zavlajkované pole, tak ukončit tah.

        //normální tah
        if (LokaceMin.Contains(newCords))
        {
            //PROHRA HRY
            GameLose(newCords);
            return true;
        }
        else if (TahyHrace.Contains(newCords)) return true;

        if (CountMinesAroundCords(newCords) == 0) GetFreeSpaces(newCords);
        else TahyHrace.Add(newCords);
        return true;
    }
    else if (input!.Trim().ToLower() == "help")
    {
        Console.Clear();

        Console.WriteLine("HELP MENU\n---------");
        Console.WriteLine("Zadej souřadnice pole ve formátu: X Y pro odkrytí dalšího pole.\nnapř.: 5 6\n");
        Console.WriteLine("Přidej k jedné ze souřadnic F k přidání, nebo odebrání vlajky z vybraného pole.\nnapř.: 5 6F\n");
        Console.WriteLine("Stiskni libovolnou klávesu pro návrat...");

        Console.ReadLine();
        return true;
    }
    //DEBUG opts
    else if (input.Trim() == "/debug showmines")
    {
        Console.Clear();

        Console.WriteLine("DEBUG: lokace min\n");
        DrawGame(VelikostPole, new(0, 0));
        Console.WriteLine("\nStiskni libovolnou klávesu pro návrat...");

        Console.ReadLine();
        return true;
    }
    else if (input.Trim() == "/debug makesingle")
    {
        for (int y = 1; y <= VelikostPole; y++)
        {
            for (int x = 1; x <= VelikostPole; x++)
            {
                if (TahyHrace.Contains(new(x, y)) || LokaceVlajek.Contains(new(x, y))) continue;
                else if (!LokaceVlajek.Contains(new(x, y)) && LokaceMin.Contains(new(x, y))) LokaceVlajek.Add(new(x, y));
                else TahyHrace.Add(new(x, y));
            }
        }
        TahyHrace.RemoveAt(new Random().Next(0, TahyHrace.Count));
        return true;
    }
    else if (input.Trim() == "/debug makewin")
    {
        GameWin();
        return true;
    }
    else if (input.Trim() == "/debug makelose")
    {
        GameLose(LokaceMin.Single());
        return true;
    }

    return false;
}
int CountMinesAroundCords(KeyValuePair<int, int> souradnice)
{
    int totalCount = 0;
    
    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            if (LokaceMin.Contains(new(souradnice.Key + x, souradnice.Value + y))) ++totalCount;
        }
    }

    return totalCount;
}
void GetFreeSpaces(KeyValuePair<int, int> space, bool continueSpread = true)
{
    if (LokaceVlajek.Contains(space)) LokaceVlajek.Remove(space);
    TahyHrace.Add(space);
    if (!continueSpread) return; //jinak neohraničí čísly vygenerovanou díru

    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            KeyValuePair<int, int> newSpace = new(space.Key + x, space.Value + y);
            if ((y == 0 && x == 0) || TahyHrace.Contains(newSpace)) continue;
            else if (space.Key + x < 1 || space.Key + x > VelikostPole ||
                space.Value + y < 1 || space.Value + y > VelikostPole) continue;

            if (LokaceMin.Contains(newSpace)) continue;
            else if (CountMinesAroundCords(newSpace) == 0) GetFreeSpaces(newSpace);
            else GetFreeSpaces(newSpace, false);
        }
    }
}

//vygeneruje miny
List<KeyValuePair<int, int>> GenMines(int pocet, int velikostPole)
{
    List<KeyValuePair<int, int>> lokaceMin = new();
    Random rnd = new();

    while (lokaceMin.Count < pocet)
    {
        KeyValuePair<int, int> newMine = new(rnd.Next(1, velikostPole + 1), rnd.Next(1, velikostPole + 1));
        if (lokaceMin.Contains(newMine)) continue;
        else lokaceMin.Add(newMine);
    }

    return lokaceMin;
}

string getFormatedTimer() => (DateTime.Now - timer).ToString("hh':'mm':'ss");
void GameWin()
{
    Console.Clear();
    Console.WriteLine($"GRATULUJI, VYHRÁL JSI!\nDohráno za: {getFormatedTimer()}\n");
    DrawGame(VelikostPole);

    Console.ReadLine();
    repeatGame = false;
}
void GameLose(KeyValuePair<int, int> naslaplaMina)
{
    Console.Clear();
    Console.WriteLine($"PROHRÁL JSI! Šlápl jsi na minu!\nPokus trval: {(DateTime.Now - timer).ToString("hh':'mm':'ss")}\n");
    DrawGame(VelikostPole, naslaplaMina);
    Console.ReadLine();

    repeatGame = false;
}