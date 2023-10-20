using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System.Text;

namespace HillEncription
{
    public class Hill
    {

        Dictionary<int, string> str { get; set; } = new Dictionary<int, string>();
        //Пара номер-буква (пронумерованный алфавит шифра)
        string stroka { get; set; }
        //Приведенная к рабочему шаблону строка-сообщение (без пробелов, знаков, все буквы заглавные)
        int[] stroka_int { get; set; }
        //stroka в чисовом виде+
        //-на основе пронумерованного афавита шифра
        string strokaModified { get; set; }
        //stroka с удаленными повторяющимися буквами

        StringBuilder alphabet = new StringBuilder("АБВГДЕЖЗИКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ");
        //русский алфавит бей ё, й

        Matrix<double> KeyMatrix;
        //ключ-матрица
        Matrix<double> KeyMatrixInverse;
        //Обратная ключ-матрице матрица
        string HilledMessage { get; set; }
        //Зашифрованное сообщение
        int[] HilledMessageToInt { get; set; }
        //Зашифрованное сообщение в виде соответствующих буквам чисел
        int[] BufferMessageToInt { get; set; }
        //Зашифрованное буфферное сообщение в виде соответствующих буквам чисел


        public Hill(string s)
        {
            //метод для приведения строки с шаблонному виду
            strokaModified = CustomStr(s);
            //метод для приведения строки с шаблонному виду
            stroka = CustomStr(s);

            //Создание алфавита
            CreateDictionary(strokaModified);

            Console.WriteLine("Получившийся алфавит");
            foreach(var el in str)
            {
                Console.WriteLine(el);
            }

            int CombinationCount = 0;
            //перебираем матрицу с помощью рандомайзера
            do
            {
                KeyMatrix = CreateKeyMatrix().Result;
                var a = Math.Abs(double.Round(KeyMatrix.Determinant()));
                CombinationCount++;

            } while (double.Round(KeyMatrix.Determinant()) != 1);
            //Создаем обратную матрицу
            KeyMatrixInverse = KeyMatrix.Inverse();

            Console.WriteLine($"Количество пересчетов ключ-матрицы {CombinationCount}");

            Console.WriteLine("Ключ-матрица");
            ShowMatrix(KeyMatrix);
            Console.WriteLine("Инверсная ключ-матрице");
            ShowMatrix(KeyMatrixInverse);
            Console.WriteLine("Перемноение этих матриц (проверка)");
            ShowMatrix(KeyMatrix * KeyMatrixInverse);
            StrokaToInt();
            //зашифрованное сообщение
            HilledMessage = EncryptionDecription(KeyMatrix, stroka_int);
            //зашифрованное сообщение в числовом виде
            HilledMessageToInt = BufferMessageToInt;

            //Расшифрованное сообщение
            string result = EncryptionDecription(KeyMatrixInverse, HilledMessageToInt);
            Console.WriteLine($"Исходное сообщение {stroka}");
            Console.WriteLine($"Зашифрованная строка {HilledMessage}");
            Console.WriteLine($"Результат расшифровки {result}");
            

            Console.ReadKey();
        }


        // Удаляет пробелы и лишние символы,меняет ё на е, й на и, делает буквы заглавными 
        static private string CustomStr(string s)
        {
            StringBuilder s_ = new StringBuilder(s.ToUpper().Replace(" ", String.Empty).Replace("Й", "И").Replace("Ё", "Е"));
            var DeleteChar = "!,.-/'\"@#$%^&*()<>;:[]{}";
            foreach (var el in DeleteChar)
            {
                s_ = s_.Replace(el.ToString(), String.Empty);
            }
            return s_.ToString();

        }


        //Берет отформатированную строку, удаляет повторяющиеся символы
        //Нумерует их по порядку от 1 до ... , удаляет использованные буквы из афавита как "уже встречавшиеся"
        //Оставшиеся буквы в алфавите нумерует продолжая порядок в алфавитной последовательности
        //Помещает получившуюся коллекцию в Dictionary str, где key = буква, а value = присвоенный ей номер
        private void CreateDictionary(string s)
        {
            strokaModified = new string(s.Distinct().ToArray());

            int counter = alphabet.Length;
            for (int i = 0; i < counter; i++)
            {
                if (i < strokaModified.Length)
                {
                    str.Add(i, strokaModified[i].ToString());
                    alphabet.Replace(strokaModified[i].ToString(), String.Empty);
                }
                else
                {
                    str.Add(i, alphabet[0].ToString());
                    alphabet.Remove(0, 1);
                }
            }

        }


        //Создает Ключ-Матрицу и заполняет ее с помощью API(Random.org)
        //Выводит полученную матрицу на экран
        private async Task<Matrix<double>> CreateKeyMatrix()
        {
            KeyMatrix = Matrix<double>.Build.Dense(4, 4);

            using (HttpClient client = new HttpClient())
            {
                string url = "https://www.random.org/integers/?num=16&min=1&max=4&col=1&base=10&format=plain&rnd=new";
                HttpResponseMessage answer = await client.GetAsync(url);
                if (answer.IsSuccessStatusCode)
                {


                    var s = await answer.Content.ReadAsStringAsync();
                    var array = s.Split('\n');

                    for (int i = 0, k = 0; i < KeyMatrix.ColumnCount; i++)
                    {
                        for (int j = 0; j < KeyMatrix.RowCount; j++, k++)
                        {
                            KeyMatrix[i, j] = int.Parse(array[k]);
                        }
                    }



                }
                else
                {
                    Console.WriteLine("Рандомайзер не отвечает");
                    Console.ReadKey();
                    for (int i = 0; i < KeyMatrix.ColumnCount; i++)
                    {
                        for (int j = 0; j < KeyMatrix.RowCount; j++)
                        {
                            KeyMatrix[i, j] = new Random().Next(100);
                        }
                    }
                }
            }
            return KeyMatrix;
        }

        public void ShowMatrix(Matrix<double> matrix)
        {
            Console.WriteLine("\tполучившаяся матрица");
            for (int i = 0; i < matrix.ColumnCount; i++)
            {
                for (int j = 0; j < matrix.RowCount; j++)
                {
                    Console.Write($"\t{Convert.ToInt32(double.Round(matrix[i, j]))}");
                }
                Console.Write("\n");
            }
        }

        //Заполняет числовую версию сообщения согласно пронумерованному словарю 
        private void StrokaToInt()
        {
            var array_size = stroka.Length % 4 == 0 ? stroka.Length : (stroka.Length + 4) / 4 * 4;
            //создает массив длинной кратной 4 в бОльшую сторону

            stroka_int = new int[array_size];
            for (int i = 0; i < stroka_int.Length; i++)
            {
                if (i < stroka.Length) { stroka_int[i] = strokaModified.IndexOf(stroka[i]); }
                else { stroka_int[i] = 26; } //26 = 'Ъ'
            }
        }


        //Перемножает одномерную матрицу 1x4 на двумерную 4x4
        private string EncryptionDecription(Matrix<double> key, int[] message)
        {
            BufferMessageToInt = new int[message.Length];
            var _HilledMessage = new StringBuilder();

            var HilledMessageToIntCounter = 0;
            for (int i = 0; i < message.Length; i += 4) //перебираем блоки по 4 значения
            {
                for (int j = 0; j < 4; j++)//перемножаем строку KeyMatrix и столбец stroka_int блока из 4 значений
                {
                    double a = 0.0;
                    for (int k = 0; k < 4; k++)
                    {
                        a += key[j, k] * (Convert.ToDouble(message[i + k]));
                        //a += Convert.ToInt32(double.Round(key[j, k])) * message[i + k];
                    }
                    int aa = (Convert.ToInt32(double.Round(a)));
                    aa %= 31;
                    aa = aa < 0 ? aa + 31 : aa;
                    _HilledMessage.Append(str[aa]); //берем результат по модулю деленый на размерность алфавита и вытаскивем соответтвующую букву из словаря
                    BufferMessageToInt[HilledMessageToIntCounter] = aa; //записываем число в числовую версию сообщения
                    HilledMessageToIntCounter++;
                }
            }
            return _HilledMessage.ToString();
        }
        static int[,] MultiplyMatrices(int[,] matrix1, int[,] matrix2)
        {
            int rows1 = matrix1.GetLength(0);
            int cols1 = matrix1.GetLength(1);
            int rows2 = matrix2.GetLength(0);
            int cols2 = matrix2.GetLength(1);

            if (cols1 != rows2)
            {
                throw new ArgumentException("Матрицы не могут быть перемножены");
            }

            int[,] result = new int[rows1, cols2];

            for (int i = 0; i < rows1; i++)
            {
                for (int j = 0; j < cols2; j++)
                {
                    for (int k = 0; k < cols1; k++)
                    {
                        result[i, j] += matrix1[i, k] * matrix2[k, j];
                    }
                }
            }

            return result;
        }
    }
}
