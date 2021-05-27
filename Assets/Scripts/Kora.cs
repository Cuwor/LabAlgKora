using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Kora : MonoBehaviour
{
    [SerializeField]
    private CanvasSampleOpenFileText firstFileLoader;

    [SerializeField]
    private CanvasSampleOpenFileText secondFileLoader;

    [SerializeField]
    private Text resultText;

    private List<List<string>> triple;
    private bool isTeacherReady = false;

    private void Awake()
    {
        firstFileLoader.FileLoaded += OnFirstFileLoaded;
        secondFileLoader.FileLoaded += OnSecondFileLoaded;
    }

    private void OnDestroy()
    {
        firstFileLoader.FileLoaded -= OnFirstFileLoaded;
        secondFileLoader.FileLoaded -= OnSecondFileLoaded;
    }

    private void OnFirstFileLoaded(string text)
    {
        var parsedCSV = new List<List<bool>>();
        foreach (var str in text.Split('\n'))
        {
            var line = str;
            parsedCSV.Add(new List<bool>());
            foreach (var a in line.Split(','))
                parsedCSV.Last().Add(Convert.ToBoolean(int.Parse(a)));
        }

        var column = 0;

        var classes = new List<bool>();

        var initialData = new List<List<bool>>();

        foreach (var p in parsedCSV)
        {
            initialData.Add(new List<bool>());
            foreach (var d in p) initialData.Last().Add(d);
        }

        foreach (var d in parsedCSV)
        {
            classes.Add(d[column]);
            d.RemoveAt(column);
        }

        triple = Learn(parsedCSV, classes);
        isTeacherReady = true;
    }

    private void OnSecondFileLoaded(string text)
    {
        StartCoroutine(WaitTeacher(text));
    }

    private IEnumerator WaitTeacher(string text)
    {
        while (!isTeacherReady) yield return null;

        var experementalData = new List<List<bool?>>();
        foreach (var str in text.Split('\n'))
        {
            var line = str;
            experementalData.Add(new List<bool?>());
            foreach (var a in line.Split(','))
                experementalData.Last().Add(Convert.ToBoolean(int.Parse(a)));
        }

        var result = Analyze(experementalData, triple);

        var resultVars = "";
        foreach (var variables in result)
        {
            foreach (var v in variables)
            {
                if (v == null)
                {
                    resultVars += "n ";
                    continue;
                }

                resultVars += Convert.ToInt32(v) + " ";
            }

            resultVars += "\n";
        }

        resultText.text = resultVars;
    }

    private static List<List<string>> Learn(List<List<bool>> data, List<bool> classes)
    {
        var triple = new List<List<string>>();

        for (var i = 0; i < data[0].Count - 2; i++)
        {
            for (var j = 0; j < data[0].Count - 1; j++)
            {
                for (var z = 0; z < data[0].Count; z++)
                    if (i != j && j != z && z != i)
                    {
                        var isExist = false;
                        foreach (var t in triple)
                            if (t[0].Contains(i.ToString()) && t[0].Contains(j.ToString()) &&
                                t[0].Contains(z.ToString()))
                            {
                                isExist = true;
                                break;
                            }

                        if (!isExist)
                        {
                            triple.Add(new List<string>());
                            triple.Last().Add(string.Format("{0},{1},{2}", i, j, z));
                            triple.Last().Add("");
                            triple.Last().Add("");
                            for (var a = 0; a < data.Count; a++)
                            {
                                var value = string.Format("{0},{1},{2}", data[a][i], data[a][j], data[a][z]);
                                var isFirstClass = false;
                                var isSecondClass = false;
                                foreach (var s in triple.Last()[1].Split(';'))
                                    if (value == s)
                                    {
                                        isFirstClass = true;
                                        break;
                                    }

                                foreach (var s in triple.Last()[2].Split(';'))
                                    if (value == s)
                                    {
                                        isSecondClass = true;
                                        break;
                                    }

                                if (classes[a] == false && !isFirstClass && !isSecondClass)
                                    triple.Last()[1] += value + ";";
                                else if (classes[a] == true && !isFirstClass && !isSecondClass)
                                    triple.Last()[2] += value + ";";
                            }
                        }
                    }
            }
        }

        return triple;
    }

    private static List<List<bool?>> Analyze(List<List<bool?>> data, List<List<string>> triple)
    {
        var result = new List<List<bool?>>();
        foreach (var d in data)
        {
            var counter1 = 0;
            var counter2 = 0;
            foreach (var t in triple)
            {
                var indexes = t[0].Split(',');
                var value = string.Format("{0},{1},{2}", d[int.Parse(indexes[0])], d[int.Parse(indexes[1])],
                    d[int.Parse(indexes[2])]);
                foreach (var t1 in t[1].Split(';'))
                    if (value == t1)
                        counter1++;

                foreach (var t2 in t[2].Split(';'))
                    if (value == t2)
                        counter2++;
            }

            result.Add(new List<bool?>());
            if (counter1 > counter2)
                result.Last().Add(false);
            else if (counter1 < counter2)
                result.Last().Add(true);
            else
                result.Last().Add(null);
            foreach (var v in d) result.Last().Add(v);
        }

        return result;
    }

    private static float Compare(List<List<bool?>> result, List<List<bool?>> data)
    {
        var counter = 0;
        var allCounter = 0;
        for (var i = 0; i < result.Count; i++)
        {
            for (var j = 0; j < result.Count; j++)
            {
                allCounter++;
                if (result[i][j] == data[i][j])
                    counter++;
            }
        }

        return counter / allCounter * 100;
    }
}