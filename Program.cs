// See https://aka.ms/new-console-template for more information

using HillEncription;

var u = false;
while(u == false) {
    Console.WriteLine("\nВвести вруную или загрузить из text.txt? (1 или 2)");
    int choise = ((int)Console.ReadKey().Key);
    Console.Clear();
    switch (choise)
    {
        case 49:
            {
                var str = new Hill(Console.ReadLine());
                u = true;
                break;
            }
        case 50:
            {
                var str = new Hill(File.ReadAllText(@"C:\Users\sofia\Desktop\ЛИМ\программы\HillEncryption\text.txt"));
                u = true;
                u =true;
                break;
            }
    }
}
