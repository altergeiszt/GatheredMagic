namespace HiddenGemBLL.Interfaces;

public interface IMathService
{
    double CalculateNPMI(double pCAndX, double pX);
    double CalculatePValue(int k, int n, int KPopulation, int NPopulation);
}