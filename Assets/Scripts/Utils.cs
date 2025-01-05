using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading.Tasks;

public static class Utils
{
    /// <summary>
    /// Проиграть рандомный звук
    /// </summary>
    /// <param name="audio">источник звука</param>
    /// <param name="arr">массив звуков</param>
    public static void PlayRandomSound(AudioSource audio, AudioClip[] arr)
    {
        if (arr.Length > 1)
        {
            // Берем произвольный звук, 0 элемент массива это прошлый проигранный звук, чтобы не выпало два одинаковых подряд
            int n = Random.Range(1, arr.Length);
            audio.clip = arr[n];
            audio.PlayOneShot(audio.clip);

            // Передвигаем проигранный звук в начало массива
            arr[n] = arr[0];
            arr[0] = audio.clip;
        }
    }

    /// <summary>
    /// Ускоренная функция подсчета растояний проецирует на 2D плоскости
    /// </summary>
    /// <param name="a">позиция 1</param>
    /// <param name="b">позиция 2</param>
    /// <returns></returns>
    public static float FastDistance(Vector3 a, Vector3 b)
    {
        if (Mathf.Abs(a.y - b.y) > 0.5f)
        {
            float xx = a.x - b.x;
            float zz = a.z - b.z;
            return xx * xx + zz * zz;
        } 
        else
        {
            return (a - b).sqrMagnitude;
        }
    }

    /// <summary>
    /// Поверяет остановился ли угол вектора
    /// </summary>
    /// <param name="angleReference">текущий вектор</param>
    /// <param name="targetAngle">нужный вектор</param>
    /// <param name="threshold">порог срабатывания</param>
    /// <returns></returns>
    public static bool IsLerpStop(float angleReference, float targetAngle, float threshold = 0.05f)
    {
        if (targetAngle < 0) targetAngle = 360 + targetAngle;
        if (targetAngle >= 360) targetAngle = targetAngle % 360;
        if (angleReference < 0.0001f && angleReference > -0.0001f) angleReference = 0; // Fixed если слишком маленькое значение после Lerp

        if (angleReference >= targetAngle - threshold && angleReference <= targetAngle + threshold)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Проверяет остановился ли угол
    /// </summary>
    /// <param name="vectorReference">текущий угол</param>
    /// <param name="vectorTarget">нужный угол</param>
    /// <returns></returns>
    public static bool IsLerpStop(Vector3 vectorReference, Vector3 vectorTarget)
    {
        return IsLerpStop(vectorReference.x, vectorTarget.x) && IsLerpStop(vectorReference.y, vectorTarget.y) && IsLerpStop(vectorReference.z, vectorTarget.z);
    }

    /// <summary>
    /// Костыль, в инспекторе показывает градусы от -180  до 180 а в методах eulerAngles выдает 360
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static float GetNormalAngle(float a, bool round = false)
    {
        a %= 360;
        a = a > 180 ? a - 360 : a;
        return (round) ? Mathf.Round(a) : a;
    }

    /// <summary>
    /// Вернуть Quaternion из вектора углов.
    /// </summary>
    /// <param name="rot"></param>
    /// <returns></returns>
    public static Quaternion GetQuaternion(Vector3 rot)
    {
        return Quaternion.Euler(GetNormalAngle(rot.x), GetNormalAngle(rot.y), GetNormalAngle(rot.z));
    }

    /// <summary>
    /// Сохранить файл в парралельном потоке.
    /// </summary>
    /// <param name="filesavepath"></param>
    /// <param name="obj"></param>
    public static async void SaveFileAsync(string filesavepath, object obj)
    {
        await Task.Run(() =>
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter { AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple };
                FileStream file = File.Create(filesavepath);
                bf.Serialize(file, obj);
                file.Close();

#if DEBUG
                Debug.Log(JsonUtility.ToJson(obj));
                File.WriteAllText(filesavepath + ".dev", JsonUtility.ToJson(obj));
#endif
            }
            finally { }
        });
    }

    public static object LoadFile(string filesavepath)
    {
        try
        {
            BinaryFormatter bf = new BinaryFormatter { AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple };
            FileStream file = File.Open(filesavepath, FileMode.Open);
            object obj = bf.Deserialize(file);
            file.Close();
            return obj;            
        } catch { return null; }
    }


    /// <summary>
    /// Выдать языки установленные в системе
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, string> GetActualLanguages()
    {
        Dictionary<string, string> retval = new Dictionary<string, string>();
        foreach (KeyValuePair<string, string> s in DictionaryLanguages)
        {
            if (File.Exists(Path.Combine(Application.streamingAssetsPath, $"{s.Key}.json"))) retval.Add(s.Key, s.Value);
        }
        return retval;
    }

    /// <summary>
    /// Имена локализации
    /// </summary>
    public static Dictionary<string, string> DictionaryLanguages = new Dictionary<string, string>
    {
        {"en", "English"},
        {"ru", "Russian"},
        {"de", "German"},
        {"fr", "French"},
        {"it", "Italian"},
        {"ja", "Japanese"},
        {"es", "Spanish"},
        {"zhs", "Chinese (Simplified)"},
        {"zht", "Chinese (Traditional)"},
        {"ar", "Arabic"},
        {"bg", "Bulgarian"},
        {"pt", "Portuguese"},
        {"hu", "Hungarian"},
        {"vi", "Vietnamese"},
        {"el", "Greek"},
        {"da", "Danish"},
        {"la", "Latin"},
        {"ko", "Korean"},
        {"nl", "Dutch"},
        {"no", "Norwegian"},
        {"pl", "Polish"},
        {"ro", "Romanian"},
        {"th", "Thai"},
        {"tr", "Turkish"},
        {"uk", "Ukrainian"},
        {"fi", "Finnish"},
        {"cs", "Czech"},
        {"sv", "Swedish"},
        {"in", "India"}
    };

    /// <summary>
    /// Поменять местами элементы массива
    /// </summary>
    /// <typeparam name="T">Тип</typeparam>
    /// <param name="objectArray">Массив</param>
    /// <param name="x">Исходный индекс</param>
    /// <param name="y">Заменяемый индекс</param>
    /// <returns></returns>
    public static bool Swap<T>(this T[] objectArray, int x, int y)
    {
        // check for out of range
        if (objectArray.Length <= y || objectArray.Length <= x) return false;

        // swap index x and y
        T temp = objectArray[x];
        objectArray[x] = objectArray[y];
        objectArray[y] = temp;

        return true;
    }

    /// <summary>
    /// Сдвинуть весь массив по типу очереди, первый элемент окажется в конце.
    /// </summary>
    /// <typeparam name="T">Тип</typeparam>
    /// <param name="objectArray">Массив</param>
    public static void Shift<T>(this T[] objectArray)
    {
        for(int i = 0; i < objectArray.Length; i++)
        {
            Swap(objectArray, i, i + 1);
        }
    }

    /// <summary>
    /// Определяет лево или право
    /// </summary>
    /// <param name="fwd">направление</param>
    /// <param name="targetDir">нормаль</param>
    /// <param name="up">Vector3.up</param>
    /// <returns></returns>
    public static int AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0f)
        {
            return 1;
        }
        else if (dir < 0f)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }

    /// <summary>
    /// Форматирует время в строк
    /// </summary>
    /// <param name="sec">время в секундах</param>
    /// <returns></returns>
    public static string FormatTime(int sec)
    {
        int h = sec / 3600;
        int m = sec / 60 - h * 60;
        int s = sec - (h * 3600 + m * 60);

        return ((h > 9) ? $"{h}:" : $"0{h}:") + ((m > 9) ? $"{m}:" : $"0{m}:") + ((s > 9) ? $"{s}" : $"0{s}");
    }


    /// <summary>
    /// Добить строку до нужной длинны с помощью разделителей
    /// </summary>
    /// <param name="s">строка</param>
    /// <param name="spl">разделитель</param>
    /// <param name="len">нужная длинна</param>
    /// <returns></returns>
    public static string AddSplits(string s, string spl, int len)
    {
        if (len <= 0) return s;

        string str = s;
        for (int i = s.Length; i < len; i++)
        {
            str = spl + str;
        }
        return str;
    }
}

public enum EDirection
{
    X,
    Y,
    Z
}

public enum EInteractionType
{
    Crossair,
    Help,
    Eye,
    Use,
    Move,
    Dialog,
    None
}

interface IInteraction
{
    void Click(RaycastHit hit);
    EInteractionType GetInteractionType(RaycastHit hit);
}