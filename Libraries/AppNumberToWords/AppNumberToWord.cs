using Service.Framework.Core.Engine;

namespace Service.Libraries.AppNumberToWords;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class AppNumberToWord
{
  private List<string> wordArray = new();
  private List<string> thousand = new();
  private decimal val;
  private string currency0;
  private string currency1;
  private List<string> valArray;
  private int decValue;
  private string decWord;
  private int numValue;
  private string numWord;
  private string valWord;
  private decimal originalVal;
  private string language;

  public AppNumberToWord(Dictionary<string, object> parameters = null)
  {
    var l = "";
    if (parameters != null && parameters.ContainsKey("clientid") && int.TryParse(parameters["clientid"].ToString(), out var clientId))
    {
      var clientLanguage = GetClientDefaultLanguage(clientId);
      if (!string.IsNullOrEmpty(clientLanguage)) l = clientLanguage;
    }

    language = string.IsNullOrEmpty(l) ? GetOption("active_language") : l;

    // Load language files (assuming some method LoadLanguage exists)
    LoadLanguage(language + "_num_words_lang", language);

    if (FileExists("custom_lang.php"))
    {
      // Unload previous custom_lang if already loaded
      UnloadLanguage("custom_lang.php");
      LoadLanguage("custom_lang", language);
    }

    language = language;

    thousand.AddRange(new string[] { "", Localize("num_word_thousand") + " ", Localize("num_word_million") + " ", Localize("num_word_billion") + " ", Localize("num_word_trillion") + " ", Localize("num_word_zillion") + " " });

    for (var i = 1; i < 100; i++) wordArray.Add(Localize("num_word_" + i));

    for (var i = 100; i <= 900; i += 100) wordArray.Add(Localize("num_word_" + i));
  }

  public string Convert(decimal inVal = 0, string inCurrency0 = "", bool inCurrency1 = true)
  {
    originalVal = inVal;
    val = inVal;
    currency0 = Localize("num_word_" + inCurrency0.ToUpper());

    var finalVal = string.Empty;

    if (inCurrency0.ToLower() == "inr")
    {
      finalVal = ConvertIndian(inVal);
    }
    else
    {
      if (currency0.Contains("num_word_")) currency0 = string.Empty;

      currency1 = inCurrency1 ? Localize("num_word_cents") : string.Empty;

      val = Math.Abs(decimal.Parse(val.ToString().Replace(",", string.Empty), CultureInfo.InvariantCulture));
      if (val > 0)
      {
        val = decimal.Parse(val.ToString("N2", CultureInfo.InvariantCulture));
        valArray = val.ToString("N2", CultureInfo.InvariantCulture).Split(',').ToList();
        decValue = int.Parse(valArray.Last());

        if (decValue > 0)
        {
          var wAnd = Localize("number_word_and");
          decWord = (wAnd == " " ? string.Empty : wAnd + " ") + wordArray[decValue] + " " + currency1;
        }

        var t = 0;
        numWord = string.Empty;

        for (var i = valArray.Count - 2; i >= 0; i--)
        {
          numValue = int.Parse(valArray[i]);

          if (numValue == 0)
          {
            numWord = " " + numWord;
          }
          else if (numValue < 100)
          {
            numWord = wordArray[numValue] + " " + thousand[t] + numWord;
            if (i == 1)
            {
              var wAnd = Localize("number_word_and");
              numWord = (wAnd == " " ? string.Empty : wAnd + " ") + numWord;
            }
          }
          else
          {
            numWord = wordArray[int.Parse(numValue.ToString()[0] + "00")] + (int.Parse(numValue.ToString().Substring(1)) > 0 ? Localize("number_word_and") != " " ? " " + Localize("number_word_and") + " " : " " : string.Empty) + wordArray[int.Parse(numValue.ToString().Substring(1))] + " " + thousand[t] + numWord;
          }

          t++;
        }

        if (!string.IsNullOrEmpty(numWord)) numWord += currency0;
      }

      valWord = numWord + " " + decWord;

      finalVal = GetOption("total_to_words_lowercase") == "1" ? valWord.ToLower() : valWord;
    }

    return ApplyFilters("before_return_num_word", finalVal, new Dictionary<string, object>
    {
      { "original_number", originalVal },
      { "currency", inCurrency0 },
      { "language", language }
    });
  }

  private string ConvertIndian(decimal num)
  {
    var count = 0;
    return ConvertNumberIndian((int)num);
  }

  private string ConvertNumberIndian(int num)
  {
    if (num < 0) return "negative" + ConvertTriIndian(-num, 0);

    if (num == 0) return "Zero";

    return ConvertTriIndian(num, 0);
  }

  private string ConvertTriIndian(int num, int tri)
  {
    var count = 0;
    var str = string.Empty;
    var r = num / 1000;
    var x = num / 100 % 10;
    var y = num % 100;

    if (count == 1)
    {
      if (x > 0)
      {
        str = Localize("num_word_" + x) + " " + (Localize("num_word_hundred") == "num_word_hundred" ? "Hundred" : Localize("num_word_hundred"));
        str += CommonLoopIndian(y, " " + Localize("number_word_and") + " ", string.Empty);
      }
      else if (r > 0)
      {
        str += CommonLoopIndian(y, " " + Localize("number_word_and") + " ", string.Empty);
      }
      else
      {
        str += CommonLoopIndian(y);
      }
    }
    else if (count == 2)
    {
      var rx = num / 10000;
      x = num / 100 % 100;
      y = num % 100;
      str += CommonLoopIndian(x, string.Empty, " " + GetLakhText(x));
      str += CommonLoopIndian(y);
      if (!string.IsNullOrEmpty(str)) str += Localize("num_word_thousand");
    }
    else if (count == 3)
    {
      if (x > 0)
      {
        str = Localize("num_word_" + x) + " " + (Localize("num_word_hundred") == "num_word_hundred" ? "Hundred" : Localize("num_word_hundred"));
        str += CommonLoopIndian(y, " " + Localize("number_word_and") + " ", " Crore ");
      }
      else if (r > 0)
      {
        str += CommonLoopIndian(y, " " + Localize("number_word_and") + " ", " Crore ");
      }
      else
      {
        str += CommonLoopIndian(y);
      }
    }

    if (r > 0) return ConvertTriIndian(r, tri + 1) + str;

    return str;
  }

  private string CommonLoopIndian(int val, string str1 = "", string str2 = "")
  {
    string[] ones = { "", Localize("num_word_1"), Localize("num_word_2"), Localize("num_word_3"), Localize("num_word_4"), Localize("num_word_5"), Localize("num_word_6"), Localize("num_word_7"), Localize("num_word_8"), Localize("num_word_9"), Localize("num_word_10"), Localize("num_word_11"), Localize("num_word_12"), Localize("num_word_13"), Localize("num_word_14"), Localize("num_word_15"), Localize("num_word_16"), Localize("num_word_17"), Localize("num_word_18"), Localize("num_word_19") };
    string[] tens = { "", "", Localize("num_word_20"), Localize("num_word_30"), Localize("num_word_40"), Localize("num_word_50"), Localize("num_word_60"), Localize("num_word_70"), Localize("num_word_80"), Localize("num_word_90") };

    var result = string.Empty;
    if (val == 0)
      result += ones[val];
    else if (val < 20)
      result += str1 + ones[val] + str2;
    else
      result += str1 + tens[val / 10] + ones[val % 10] + str2;

    return result;
  }

  private string GetLakhText(int x)
  {
    var key = x <= 1 ? "num_word_lakh" : "num_word_lakhs";
    var text = Localize(key);

    return text == key ? x <= 1 ? "Lakh" : "Lakhs" : text;
  }

  // Placeholder methods for localization, loading language, etc.
  private string Localize(string key)
  {
    return key;
    // Implement localization logic here
  }

  private void LoadLanguage(string file, string language)
  {
  } // Implement language loading logic here

  private void UnloadLanguage(string file)
  {
  } // Implement language unloading logic here

  private bool FileExists(string path)
  {
    return false;
    // Implement file existence check here
  }

  private string GetClientDefaultLanguage(int clientId)
  {
    return "en";
    // Implement client default language retrieval here
  }

  private string GetOption(string key)
  {
    return "1";
    // Implement option retrieval logic here
  }

  private string ApplyFilters(string filter, string value, Dictionary<string, object> args)
  {
    return value;
    // Implement filter application logic here
  }
}

public static class AppNumberToWordExtensions
{
  public static string app_number_to_word(this LibraryBase lib, object currency, string key)
  {
    return new AppNumberToWord().Convert();
  }
}
