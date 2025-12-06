using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        int n = 1000; // Количество симулируемых значений для каждой СВ
        Random rand = new Random(); // Генератор случайных чисел

        // Задача 1: Нормальное распределение N(-4, 4)
        double meanNormal = -4; // Математическое ожидание
        double varianceNormal = 4; // Дисперсия
        double stdDevNormal = Math.Sqrt(varianceNormal); // Стандартное отклонение
        List<double> normalSamples = GenerateNormalSamples(rand, n, meanNormal, stdDevNormal);
        PrintStats("Нормальное распределение", normalSamples, meanNormal, varianceNormal);

        // Задача 2: Экспоненциальное распределение E(0.5)
        double lambdaExponential = 0.5; // Параметр лямбда (интенсивность)
        List<double> exponentialSamples = GenerateExponentialSamples(rand, n, lambdaExponential);
        double trueMeanExponential = 1 / lambdaExponential; // Истинное математическое ожидание
        double trueVarianceExponential = 1 / (lambdaExponential * lambdaExponential); // Истинная дисперсия
        PrintStats("Экспоненциальное распределение", exponentialSamples, trueMeanExponential, trueVarianceExponential);

        // Задача 2: Логистическое распределение LG(0, 1.5)
        double aLogistic = 0; // Параметр положения
        double bLogistic = 1.5; // Параметр масштаба
        List<double> logisticSamples = GenerateLogisticSamples(rand, n, aLogistic, bLogistic);
        double trueMeanLogistic = aLogistic; // Истинное математическое ожидание
        double trueVarianceLogistic = (Math.PI * Math.PI * bLogistic * bLogistic) / 3; // Истинная дисперсия
        PrintStats("Логистическое распределение", logisticSamples, trueMeanLogistic, trueVarianceLogistic);

        // Задача 5: Проверка критерия Колмогорова для каждой выборки
        Console.WriteLine("\n--- Проверка критерия Колмогорова (α=0.05) ---");
        TestKolmogorovSmirnov(normalSamples, new NormalDistribution(meanNormal, varianceNormal));
        TestKolmogorovSmirnov(exponentialSamples, new ExponentialDistribution(lambdaExponential));
        TestKolmogorovSmirnov(logisticSamples, new LogisticDistribution(aLogistic, bLogistic));

        // Задача 5: Проверка критерия Пирсона для каждой выборки
        Console.WriteLine("\n--- Проверка критерия Пирсона (α=0.05) ---");
        TestChiSquared(normalSamples, new NormalDistribution(meanNormal, varianceNormal));
        TestChiSquared(exponentialSamples, new ExponentialDistribution(lambdaExponential));
        TestChiSquared(logisticSamples, new LogisticDistribution(aLogistic, bLogistic));

        // Задачи 3 и 4: Эмпирическая проверка вероятности ошибки I рода
        Console.WriteLine("\n--- Эмпирическая проверка вероятности ошибки I рода (1000 симуляций) ---");
        CheckTypeIErrors(1000);
    }

    /// <summary>
    /// Генерация выборки из нормального распределения N(μ, σ²) методом Бокса-Мюллера
    /// </summary>
    /// <param name="rand">Генератор случайных чисел</param>
    /// <param name="n">Количество значений</param>
    /// <param name="mean">Математическое ожидание</param>
    /// <param name="stdDev">Стандартное отклонение</param>
    /// <returns>Список сгенерированных значений</returns>
    static List<double> GenerateNormalSamples(Random rand, int n, double mean, double stdDev)
    {
        var samples = new List<double>();
        for (int i = 0; i < n; i++)
        {
            // Генерация двух независимых равномерных случайных величин
            double u1 = rand.NextDouble();
            double u2 = rand.NextDouble();

            // Преобразование в нормальную величину (метод Бокса-Мюллера)
            // z ~ N(0,1), затем масштабирование: x = μ + σ*z
            double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            samples.Add(mean + stdDev * z);
        }
        return samples;
    }

    /// <summary>
    /// Генерация выборки из экспоненциального распределения E(λ)
    /// </summary>
    /// <param name="rand">Генератор случайных чисел</param>
    /// <param name="n">Количество значений</param>
    /// <param name="lambda">Параметр λ (интенсивность)</param>
    /// <returns>Список сгенерированных значений</returns>
    static List<double> GenerateExponentialSamples(Random rand, int n, double lambda)
    {
        var samples = new List<double>();
        for (int i = 0; i < n; i++)
        {
            // Метод обратного преобразования: x = -ln(u)/λ
            // u ~ U(0,1) => x ~ E(λ)
            samples.Add(-Math.Log(rand.NextDouble()) / lambda);
        }
        return samples;
    }

    /// <summary>
    /// Генерация выборки из логистического распределения LG(a, b)
    /// </summary>
    /// <param name="rand">Генератор случайных чисел</param>
    /// <param name="n">Количество значений</param>
    /// <param name="a">Параметр положения</param>
    /// <param name="b">Параметр масштаба</param>
    /// <returns>Список сгенерированных значений</returns>
    static List<double> GenerateLogisticSamples(Random rand, int n, double a, double b)
    {
        var samples = new List<double>();
        for (int i = 0; i < n; i++)
        {
            // Метод обратного преобразования: x = a + b*ln(u/(1-u))
            // u ~ U(0,1) => x ~ LG(a,b)
            double u = rand.NextDouble();
            samples.Add(a + b * Math.Log(u / (1 - u)));
        }
        return samples;
    }

    /// <summary>
    /// Вывод статистических характеристик выборки с сравнением с теоретическими значениями
    /// </summary>
    /// <param name="name">Название распределения</param>
    /// <param name="samples">Выборка случайных величин</param>
    /// <param name="trueMean">Истинное математическое ожидание</param>
    /// <param name="trueVariance">Истинная дисперсия</param>
    static void PrintStats(string name, List<double> samples, double trueMean, double trueVariance)
    {
        double sampleMean = samples.Average(); // Выборочное среднее (несмещенная оценка)
        double sampleVariance = CalculateSampleVariance(samples); // Выборочная дисперсия (несмещенная: деление на n-1)

        Console.WriteLine($"\n{name}:");
        Console.WriteLine($"  Выборочное среднее: {sampleMean:F4} (теоретическое: {trueMean:F4})");
        Console.WriteLine($"  Отклонение среднего: {Math.Abs(sampleMean - trueMean):F4}");
        Console.WriteLine($"  Выборочная дисперсия: {sampleVariance:F4} (теоретическая: {trueVariance:F4})");
        Console.WriteLine($"  Отклонение дисперсии: {Math.Abs(sampleVariance - trueVariance):F4}");
    }

    /// <summary>
    /// Вычисление несмещенной оценки дисперсии (деление на n-1)
    /// </summary>
    /// <param name="samples">Выборка случайных величин</param>
    /// <returns>Несмещенная оценка дисперсии</returns>
    static double CalculateSampleVariance(List<double> samples)
    {
        double mean = samples.Average();
        // Сумма квадратов отклонений от среднего, деленная на (n-1)
        return samples.Sum(x => Math.Pow(x - mean, 2)) / (samples.Count - 1);
    }

    /// <summary>
    /// Проверка гипотезы о соответствии выборки теоретическому распределению с помощью критерия Колмогорова
    /// </summary>
    /// <param name="samples">Выборка случайных величин</param>
    /// <param name="distribution">Теоретическое распределение</param>
    static void TestKolmogorovSmirnov(List<double> samples, IDistribution distribution)
    {
        samples.Sort(); // Сортировка выборки для построения эмпирической CDF
        double dPlus = 0, dMinus = 0;
        int n = samples.Count;

        // Вычисление максимальных отклонений D+ и D- между эмпирической и теоретической CDF
        for (int i = 0; i < n; i++)
        {
            double empiricalCdf = (i + 1) / (double)n; // F_n(x_i) = (i+1)/n
            double theoreticalCdf = distribution.Cdf(samples[i]); // F(x_i)

            // D+ = max{F_n(x_i) - F(x_i)}
            dPlus = Math.Max(dPlus, empiricalCdf - theoreticalCdf);
            // D- = max{F(x_i) - F_{n-1}(x_i)} = max{F(x_i) - i/n}
            dMinus = Math.Max(dMinus, theoreticalCdf - (i / (double)n));
        }

        double d = Math.Max(dPlus, dMinus); // Статистика критерия D = max(D+, D-)
        double criticalValue = 1.36 / Math.Sqrt(n); // Критическое значение для α=0.05 (приближение для больших n)
        bool reject = d > criticalValue; // Отклонение нулевой гипотезы

        Console.WriteLine($"{distribution.GetType().Name.Replace("Distribution", "")}:");
        Console.WriteLine($"  Статистика D = {d:F4} (крит. значение = {criticalValue:F4})");
        Console.WriteLine($"  Гипотеза о соответствии: {(reject ? "Отвергнута" : "Принята")}");
    }

    /// <summary>
    /// Проверка гипотезы с помощью критерия Пирсона (χ²)
    /// Исправлено: добавлена защита от передачи 0 и 1 в InverseCdf
    /// </summary>
    /// <param name="samples">Выборка случайных величин</param>
    /// <param name="distribution">Теоретическое распределение</param>
    static void TestChiSquared(List<double> samples, IDistribution distribution)
    {
        int k = 10; // Количество интервалов (рекомендуется брать √n, но не менее 5-10)
        double epsilon = 1e-6; // Маленькое значение для защиты от 0 и 1
        double expected = samples.Count / (double)k; // Ожидаемое количество в каждом интервале
        double[] observed = new double[k]; // Наблюдаемые частоты

        // Вычисление границ интервалов так, чтобы вероятность попадания в каждый была равна 1/k
        double[] boundaries = new double[k + 1];
        for (int i = 0; i <= k; i++)
        {
            // Избегаем значений 0 и 1, используя epsilon
            double p = epsilon + (1 - 2 * epsilon) * i / (double)k;
            boundaries[i] = distribution.InverseCdf(p);
        }

        // Подсчет наблюдаемых частот в интервалах
        foreach (double x in samples)
        {
            for (int i = 0; i < k; i++)
            {
                if (x >= boundaries[i] && x < boundaries[i + 1])
                {
                    observed[i]++;
                    break;
                }
            }
        }

        // Вычисление статистики χ² = Σ(O_i - E_i)² / E_i
        double chiSquared = 0;
        for (int i = 0; i < k; i++)
        {
            chiSquared += Math.Pow(observed[i] - expected, 2) / expected;
        }

        // Критическое значение для χ² с k-1 степенями свободы при α=0.05
        // Для k=10: df=9, крит. значение = 16.919 (из таблицы χ²)
        double criticalValue = 16.919;
        bool reject = chiSquared > criticalValue;

        Console.WriteLine($"{distribution.GetType().Name.Replace("Distribution", "")}:");
        Console.WriteLine($"  Статистика χ² = {chiSquared:F4} (крит. значение = {criticalValue:F4})");
        Console.WriteLine($"  Гипотеза о соответствии: {(reject ? "Отвергнута" : "Принята")}");
    }

    /// <summary>
    /// Эмпирическая проверка вероятности ошибки I рода (отклонение верной гипотезы)
    /// </summary>
    /// <param name="numSimulations">Количество симуляций</param>
    static void CheckTypeIErrors(int numSimulations)
    {
        Random rand = new Random();
        int n = 1000; // Размер выборки
        int k = 10; // Количество интервалов для χ²
        double criticalValueKS = 1.36 / Math.Sqrt(n); // Крит. значение для Колмогорова (α=0.05)
        double criticalValueChi2 = 16.919; // Крит. значение для Пирсона (df=9, α=0.05)

        // Счетчики отклонений нулевой гипотезы (когда распределение верное)
        int[] ksRejections = new int[3]; // [Нормальное, Экспоненциальное, Логистическое]
        int[] chi2Rejections = new int[3];

        for (int sim = 0; sim < numSimulations; sim++)
        {
            // Генерация данных из правильных распределений
            var normalSamples = GenerateNormalSamples(rand, n, -4, 2);
            var expSamples = GenerateExponentialSamples(rand, n, 0.5);
            var logSamples = GenerateLogisticSamples(rand, n, 0, 1.5);

            // Проверка для нормального распределения
            double ksNormal = ComputeKSStatistic(normalSamples, new NormalDistribution(-4, 4));
            double chi2Normal = ComputeChi2Statistic(normalSamples, new NormalDistribution(-4, 4), k);
            if (ksNormal > criticalValueKS) ksRejections[0]++;
            if (chi2Normal > criticalValueChi2) chi2Rejections[0]++;

            // Проверка для экспоненциального распределения
            double ksExp = ComputeKSStatistic(expSamples, new ExponentialDistribution(0.5));
            double chi2Exp = ComputeChi2Statistic(expSamples, new ExponentialDistribution(0.5), k);
            if (ksExp > criticalValueKS) ksRejections[1]++;
            if (chi2Exp > criticalValueChi2) chi2Rejections[1]++;

            // Проверка для логистического распределения
            double ksLog = ComputeKSStatistic(logSamples, new LogisticDistribution(0, 1.5));
            double chi2Log = ComputeChi2Statistic(logSamples, new LogisticDistribution(0, 1.5), k);
            if (ksLog > criticalValueKS) ksRejections[2]++;
            if (chi2Log > criticalValueChi2) chi2Rejections[2]++;
        }

        // Вывод результатов: доля отклонений должна быть близка к α=0.05
        string[] distros = { "Нормальное", "Экспоненциальное", "Логистическое" };

        Console.WriteLine("\nВероятность ошибки I рода для критерия Колмогорова (ожидаемо ≈0.05):");
        for (int i = 0; i < 3; i++)
        {
            double rate = ksRejections[i] / (double)numSimulations;
            Console.WriteLine($"  {distros[i]}: {rate:F4} (абс. ошибка: {Math.Abs(rate - 0.05):F4})");
        }

        Console.WriteLine("\nВероятность ошибки I рода для критерия Пирсона (ожидаемо ≈0.05):");
        for (int i = 0; i < 3; i++)
        {
            double rate = chi2Rejections[i] / (double)numSimulations;
            Console.WriteLine($"  {distros[i]}: {rate:F4} (абс. ошибка: {Math.Abs(rate - 0.05):F4})");
        }
    }

    /// <summary>
    /// Вычисление статистики D для критерия Колмогорова
    /// </summary>
    static double ComputeKSStatistic(List<double> samples, IDistribution distribution)
    {
        samples.Sort();
        double dPlus = 0, dMinus = 0;
        int n = samples.Count;

        for (int i = 0; i < n; i++)
        {
            double empiricalCdf = (i + 1) / (double)n;
            double theoreticalCdf = distribution.Cdf(samples[i]);
            dPlus = Math.Max(dPlus, empiricalCdf - theoreticalCdf);
            dMinus = Math.Max(dMinus, theoreticalCdf - i / (double)n);
        }

        return Math.Max(dPlus, dMinus);
    }

    /// <summary>
    /// Вычисление статистики χ² для критерия Пирсона
    /// Исправлено: добавлена защита от передачи 0 и 1 в InverseCdf
    /// </summary>
    static double ComputeChi2Statistic(List<double> samples, IDistribution distribution, int k)
    {
        double epsilon = 1e-6; // Защита от 0 и 1
        double expected = samples.Count / (double)k;
        double[] observed = new double[k];
        double[] boundaries = new double[k + 1];

        // Определение границ интервалов (равновероятных, но с защитой от 0 и 1)
        for (int i = 0; i <= k; i++)
        {
            double p = epsilon + (1 - 2 * epsilon) * i / (double)k;
            boundaries[i] = distribution.InverseCdf(p);
        }

        // Подсчет наблюдаемых частот
        foreach (double x in samples)
        {
            for (int i = 0; i < k; i++)
            {
                if (x >= boundaries[i] && x < boundaries[i + 1])
                {
                    observed[i]++;
                    break;
                }
            }
        }

        // Вычисление χ²-статистики
        double chi2 = 0;
        for (int i = 0; i < k; i++)
        {
            chi2 += Math.Pow(observed[i] - expected, 2) / expected;
        }
        return chi2;
    }
}

// Интерфейс и реализации распределений

/// <summary>
/// Интерфейс для работы с теоретическими распределениями
/// </summary>
public interface IDistribution
{
    /// <summary>
    /// Вычисление функции распределения F(x) = P(X ≤ x)
    /// </summary>
    double Cdf(double x);

    /// <summary>
    /// Вычисление обратной функции распределения (квантиль)
    /// </summary>
    double InverseCdf(double p);
}

/// <summary>
/// Реализация нормального распределения N(μ, σ²)
/// </summary>
public class NormalDistribution : IDistribution
{
    private double mean;
    private double stdDev;

    public NormalDistribution(double mean, double variance)
    {
        this.mean = mean;
        this.stdDev = Math.Sqrt(variance);
    }

    /// <summary>
    /// Аппроксимация функции распределения нормального закона
    /// Используется полиномиальная аппроксимация для упрощения вычислений
    /// </summary>
    public double Cdf(double x)
    {
        double z = (x - mean) / stdDev;
        if (z < -8.0) return 0.0; // Практически 0
        if (z > 8.0) return 1.0;  // Практически 1

        // Аппроксимация для стандартного нормального распределения
        double t = 1.0 / (1.0 + 0.2316419 * Math.Abs(z));
        double d = 0.3989423 * Math.Exp(-z * z / 2);
        double cdf = d * t * (0.3193815 + t * (-0.3565638 + t * (1.781478 + t * (-1.821256 + t * 1.330274))));

        return z >= 0 ? 1 - cdf : cdf;
    }

    /// <summary>
    /// Обратная функция распределения (квантиль) для нормального закона
    /// Используется аппроксимация для вычисления квантилей
    /// </summary>
    public double InverseCdf(double p)
    {
        if (p <= 0 || p >= 1)
            throw new ArgumentOutOfRangeException("p", $"Должно быть 0 < p < 1, получено {p}");

        double q = p - 0.5;
        double r = q * q;

        // Аппроксимация квантилей стандартного нормального распределения
        double z = q * (((((1.061405429 * r - 5.447609879) * r + 13.32224617) * r - 10.67207815) * r + 3.004592610) * r + 0.28442269) /
                 (((((2.484063053 * r - 11.17627456) * r + 19.60127404) * r - 13.2900398) * r + 3.02655724) * r + 0.2942476);

        return mean + stdDev * z;
    }
}

/// <summary>
/// Реализация экспоненциального распределения E(λ)
/// </summary>
public class ExponentialDistribution : IDistribution
{
    private double lambda;

    public ExponentialDistribution(double lambda)
    {
        this.lambda = lambda;
    }

    /// <summary>
    /// Функция распределения E(λ): F(x) = 1 - e^(-λx) для x ≥ 0
    /// </summary>
    public double Cdf(double x) => x < 0 ? 0 : 1 - Math.Exp(-lambda * x);

    /// <summary>
    /// Обратная функция распределения: x = -ln(1-p)/λ
    /// </summary>
    public double InverseCdf(double p) => -Math.Log(1 - p) / lambda;
}

/// <summary>
/// Реализация логистического распределения LG(a, b)
/// </summary>
public class LogisticDistribution : IDistribution
{
    private double a;
    private double b;

    public LogisticDistribution(double a, double b)
    {
        this.a = a;
        this.b = b;
    }

    /// <summary>
    /// Функция распределения LG(a,b): F(x) = 1/(1 + e^(-(x-a)/b))
    /// </summary>
    public double Cdf(double x) => 1 / (1 + Math.Exp(-(x - a) / b));

    /// <summary>
    /// Обратная функция распределения: x = a + b*ln(p/(1-p))
    /// </summary>
    public double InverseCdf(double p) => a + b * Math.Log(p / (1 - p));
}
