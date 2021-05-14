using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lattice_app
{
    public class LLL
    {
        List<Vector_n> vectors = new List<Vector_n>();
        List<List<Vector_n>> list_vectors = new List<List<Vector_n>>();

        public LLL(List<Vector_n> v)
        {
            vectors = v;
        }
        public static void GramSchmidt(double[,] basis, out double[,] orthogonalBasis, double[,] b)
        {
            int dimension = basis.GetLength(0);

            orthogonalBasis = new double[dimension, dimension];
            //    orthogonalBasis[0] = basis[0];
            //    b = new Matrix(dimension, dimension);

            //for (int n = 1; n < dimension; n++)
            //{
            //    double[] sum = new double[dimension];
            //    double[] ortVector;

            //    for (int j = 0; j < n; j++)
            //    {
            //        ortVector = orthogonalBasis[j];
            //        b[n, j] = basis[n].DotProduct(ortVector) / ortVector.DotProduct(ortVector);
            //    }

            //    for (int s = 0; s < n; s++)
            //    {
            //        sum += b[n, s] * orthogonalBasis[s];
            //    }

            //    orthogonalBasis[n] = basis[n] - sum;
            //}

        }
        public List<List<Vector_n>> Gram_S(List<Vector_n> v)
        {
            List<Vector_n> coef = new List<Vector_n>();
            List<Vector_n> u_z = new List<Vector_n>();
            coef.Add(v[0]);
            u_z.Add(v[0]);
            for (int i = 1; i < v.Count; i++)
            {
                Vector_n c = new Vector_n(v[i].vector);
                Vector_n u = new Vector_n(v[i].vector);
                for (int j = 0; j < i; j++)
                {
                    u -= Proj_z(u_z[j], v[i]);
                    //c -= Proj_r(coef[j], v[i]);
                }
                coef.Add(c);
                u_z.Add(u);
            }
            return new List<List<Vector_n>>() { u_z, coef };
        }
        public Dictionary<double[,], List<Vector_n>> Gram_Schmidt(List<Vector_n> v)
        {
            List<Vector_n> ortho_vectors = new List<Vector_n>();
            double[,] coef = new double[v.Count, v.Count];
            for (int i = 0; i < v.Count; i++)
                for (int j = 0; j < v.Count; j++)
                    coef[i, j] = 0;
            ortho_vectors.Add(v[0]);
            for (int i = 1; i < v.Count; i++)
            {
                Vector_n orh_vec = new Vector_n(v[i].vector);

                for (int j = 0; j < i; j++)
                {
                    Dictionary<double, Vector_n> res = new Dictionary<double, Vector_n>();
                    res = Proj_r(ortho_vectors[j], v[i]);
                    orh_vec -= res.ElementAt(0).Value;
                    coef[i, j] = res.ElementAt(0).Key;
                 }
                 ortho_vectors.Add(orh_vec);
             }           
            return new Dictionary<double[,], List<Vector_n>>(){ { coef, ortho_vectors } }; 
        }
        public IEnumerator<Vector_n> GetEnumerator()
        {
            foreach (var el in vectors)
                yield return el;
        }
        public Vector_n this[int i]
        {
            get { return vectors[i]; }
            set { vectors[i] = value; }
        }

        public Vector_n Sum_Vectors(Vector_n[] vctrs)
        {
            Vector_n res = vctrs[0];
            for (int i = 1; i < vctrs.Length; i++)
                res += vctrs[i];
            return res;
        }
        public List<List<Vector_n>> prob_LLL_alg(List<Vector_n> basis, bool flag)
        {
            List<List<Vector_n>> full_collecion = new List<List<Vector_n>>() { new List<Vector_n>()};// список состояний
            Dictionary<double[,], List<Vector_n>> gr_sch = new Dictionary<double[,], List<Vector_n>>();
            double[,] coef_Gram_Schmidt = new LLL(basis).Gram_Schmidt(basis).ElementAt(0).Key;
            List<Vector_n> ortho_vectors = new LLL(basis).Gram_Schmidt(basis).ElementAt(0).Value;
            //full_collecion
            int k = 1;
            double δ = 0.75;
            while (k < basis.Count)
            {
                for (int j = k-1; j > -1; j--)
                {
                    if (Math.Abs(coef_Gram_Schmidt[k,j]) > 0.5)
                    {
                        basis[k] -= Closest_integer((coef_Gram_Schmidt[k,j])) * basis[j];
                        gr_sch = new LLL(basis).Gram_Schmidt(basis);
                        coef_Gram_Schmidt = gr_sch.ElementAt(0).Key;
                        ortho_vectors = gr_sch.ElementAt(0).Value;
                        full_collecion[0].Add(basis[0]);
                        full_collecion[0].Add(basis[1]);
                    }
                }
                if (ortho_vectors[k].Length* ortho_vectors[k].Length < (δ - Math.Pow(coef_Gram_Schmidt[k,k - 1], 2)) * ortho_vectors[k - 1].Length* ortho_vectors[k - 1].Length)
                {
                    Swap(basis[k], basis[k - 1], ref basis);
                    gr_sch = new LLL(basis).Gram_Schmidt(basis);
                    coef_Gram_Schmidt = gr_sch.ElementAt(0).Key;
                    ortho_vectors = gr_sch.ElementAt(0).Value;
                    full_collecion[0].Add(basis[0]);
                    full_collecion[0].Add(basis[1]);                
                    k = Math.Max(k - 1, 1);
                }
                else
                {
                    k++;
                }
            }
            if (flag is false)
                return new List<List<Vector_n>>() { basis, ortho_vectors };
            else return full_collecion; 
        }


        public List<List<Vector_n>> LLL_alg(List<Vector_n> basis, bool flag)
        {
            List<Vector_n> new_bas = new List<Vector_n>();
            List<List<Vector_n>> full_collecion = new List<List<Vector_n>>();// список состояний
            List < List <Vector_n>> coef_Gram_Schmidt = new LLL(basis).Gram_S(basis);
            full_collecion = coef_Gram_Schmidt;
            int k = 1;
            double δ = 0.75;
            while (k <= basis.Count-1)
            {
                //for (int j = k; j > -1; j--)
                //{
                //    double bd = Math.Abs(coef_Gram_Schmidt[k][j]);
                //    if (Math.Abs(coef_Gram_Schmidt[k][j]) > 0.5)
                //    {
                //        basis[k] -= Closest_integer(Math.Abs(coef_Gram_Schmidt[k][j])) * basis[j];
                //        coef_Gram_Schmidt = new LLL(basis).Gram_Schmidt(basis);
                //       // full_collecion.Add(basis);
                //        //full_collecion[0].Add(basis[1]);
                //    }
                //}
                double u = new LLL(basis).Find_μ(coef_Gram_Schmidt[0][k - 1],coef_Gram_Schmidt[0][k]);
                if (Length_squared(coef_Gram_Schmidt[0][k]) < (δ - Math.Pow(u, 2)) * Length_squared(coef_Gram_Schmidt[0][k - 1]))
                {
                    new_bas = Swap(coef_Gram_Schmidt[0][k], coef_Gram_Schmidt[0][k - 1], coef_Gram_Schmidt[0]);
                    coef_Gram_Schmidt = new LLL(basis).Gram_S(new_bas);
                    full_collecion[0].Add(coef_Gram_Schmidt[0][0]);
                    full_collecion[0].Add(coef_Gram_Schmidt[0][1]);// добавляем следующее состояние базиса                  
                    k = Math.Max(k - 1, 1);
                }
                else
                {
                    k++;
                }
            }
            if (flag is false)
                return coef_Gram_Schmidt;
            else return full_collecion;
        }
        public string LLL_alg_str(List<Vector_n> basis)
        {
            string result = "";
            List<List<Vector_n>> full_collecion = new List<List<Vector_n>>() { new List<Vector_n>() };// список состояний
            Dictionary<double[,], List<Vector_n>> gr_sch = new Dictionary<double[,], List<Vector_n>>();
            double[,] coef_Gram_Schmidt = new LLL(basis).Gram_Schmidt(basis).ElementAt(0).Key;
            List<Vector_n> ortho_vectors = new LLL(basis).Gram_Schmidt(basis).ElementAt(0).Value;

            result += "Basis after Gram-Schmidt algorithm:" + "\n"; ;
            result += "Gram_Schmidt's coefficients:" + "\n";
            foreach (var v in coef_Gram_Schmidt)
                result += v.ToString() + "\n";
            result += "Orthogonal vectors:" + "\n";
            foreach (var v in ortho_vectors)
                result += v.ToString() + "\n";
            result += "δ = 0.75" + "\n";
            int k = 1;
            double δ = 0.75;
            while (k < basis.Count)
            {
                for (int j = k-1; j > -1; j--)
                {
                    if (Math.Abs(coef_Gram_Schmidt[k,j]) > 0.5)
                    {
                        result += "\n" + Math.Abs(coef_Gram_Schmidt[k,j]).ToString() + "> 0.5" + "\n";
                        result +=  basis[k].ToString() + "-= " + (Closest_integer(Math.Abs(coef_Gram_Schmidt[k,j])) * basis[j]).ToString()+ "\n";
                        basis[k] -= Closest_integer(Math.Abs(coef_Gram_Schmidt[k,j])) * basis[j];
                        gr_sch = new LLL(basis).Gram_Schmidt(basis);
                        result += "Calculating new Gram_Schmidt's coefficients and orthogonal vectors" + "\n";
                        coef_Gram_Schmidt = gr_sch.ElementAt(0).Key;
                        ortho_vectors = gr_sch.ElementAt(0).Value; ;
                        for (int i = 0; i < coef_Gram_Schmidt.GetLength(0); i++)
                        {
                            for (int y = 0; y < coef_Gram_Schmidt.GetLength(0); y++)
                                result += coef_Gram_Schmidt[i, y].ToString() + " ";
                            result += "\n";
                        }
                        result += "Orthogonal vectors:" + "\n";
                        foreach (var v in ortho_vectors)
                            result += v.ToString() + "\n";
                        full_collecion[0].Add(basis[0]);
                        full_collecion[0].Add(basis[1]);
                    }
                }
                result += "\n" + k.ToString() + "<=" + (basis.Count - 1).ToString() + "\n";
                if (ortho_vectors[k].Length < (δ - Math.Pow(coef_Gram_Schmidt[k,k - 1], 2)) * ortho_vectors[k - 1].Length)
                {
                    result += ortho_vectors[k].Length.ToString() + "<" + ((δ - Math.Pow(coef_Gram_Schmidt[k,k - 1], 2)) * ortho_vectors[k - 1].Length).ToString() + "\n";
                    result += "Swap " + basis[k].ToString() + " and " + basis[k - 1].ToString() + "\n";
                    Swap(basis[k], basis[k - 1], ref basis);
                    gr_sch = new LLL(basis).Gram_Schmidt(basis);
                    result += "Calculating new Gram_Schmidt's coefficients and orthogonal vectors" + "\n";
                    coef_Gram_Schmidt = gr_sch.ElementAt(0).Key;
                    ortho_vectors = gr_sch.ElementAt(0).Value;
                    for (int i = 0; i < coef_Gram_Schmidt.GetLength(0); i++)
                    {
                        for (int y = 0; y < coef_Gram_Schmidt.GetLength(0); y++)
                            result += coef_Gram_Schmidt[i, y].ToString() + " ";
                        result += "\n";
                    }
                    result += "Orthogonal vectors:" + "\n";
                    foreach (var v in ortho_vectors)
                        result += v.ToString() + "\n";
                    result += "k = Math.Max(" + (k - 1).ToString() + ",1) is " + (Math.Max(k - 1, 1)).ToString() + "\n";
                    k = Math.Max(k - 1, 1);
                }
                else
                {
                    result += ortho_vectors[k].Length.ToString() + ">=" + ((δ - Math.Pow(coef_Gram_Schmidt[k,k - 1], 2)) * ortho_vectors[k - 1].Length).ToString() + "\n";
                    k++;
                    result += "k = k + 1 = " + k.ToString() + "\n";
                }
            }
            result += "\n" + k.ToString() + ">" + (basis.Count - 1).ToString() + "\n";
            result += "Result Gram_Schmidt's coefficients:" + "\n";
            for (int i = 0; i < coef_Gram_Schmidt.GetLength(0); i++)
            {
                for (int j = 0; j < coef_Gram_Schmidt.GetLength(0); j++)
                    result += coef_Gram_Schmidt[i, j].ToString() + " ";
                result += "\n";
            }
            result += "Result Orthogonal vectors:" + "\n";
            foreach (var v in ortho_vectors)
                result += v.ToString() + "\n";
            result += "Result LLL-basis:" + "\n";
            foreach (var v in basis)
                result += v.ToString() + "\n";
            return result;
        }
        public List<Vector_n> prob_Babai_alg(List<Vector_n> basis, Point_n point)
        {
            LLL gram = new LLL(basis);
            var b = gram.prob_LLL_alg(basis, false); //b[0] - lll-базис, b[1] - коэффициента Грама-Шмидта
            Point_n x = point;// введённая точка
            double[] r = new double[point.Count];
            double m_z = 0;
            List<double> m = new List<double>();
            List<Vector_n> res = new List<Vector_n>();
            for (int i = point.Count - 1; i >= 0; i--)
            {
                r[i] = b[1][i] * x / (b[1][i] * b[1][i]);
                m_z = Closest_integer(r[i]);
                m.Add(m_z);
                x = x - (r[i] - m_z) * b[1][i];
                if (i > 0)
                    x -= m_z * b[0][i];
                res.Add(new Vector_n(new List<double>() { Math.Round(x[0]), Math.Round(x[1]) }));
            }
            m.Reverse();
            res.Add(b[0] * new Vector_n(m));
            return res;
        }

        public List<Vector_n> Babai_alg(List<Vector_n> basis, Point_n point)
        {
            LLL gram = new LLL(basis);
            var b = gram.prob_LLL_alg(basis,false); //b[0] - lll-базис, b[1] - коэффициента Грама-Шмидта
            Point_n x = point;// введённая точка
            double[] r = new double[point.Count];
            double m_z = 0;
            List<double> m = new List<double>();
            List<Vector_n> res = new List<Vector_n>();
            for (int i = point.Count - 1; i >= 0; i--)
            {
                r[i] = b[0][i] * x / (b[0][i]* b[1][i]);
                m_z = Closest_integer(r[i]);
                m.Add(m_z);
                if (i > 0)
                x = x - m_z * b[1][i] - (r[i] - m_z) * b[0][i];
                res.Add(new Vector_n(new List<double>() {Math.Round(x[0]), Math.Round(x[1])}));
            }
            m.Reverse();
            res.Add(b[0] * new Vector_n(m));
            return res ;
        }

        public string Babai_alg_str(List<Vector_n> basis, Point_n point)
        {
            string result = "";
            LLL gram = new LLL(basis);
            var b = gram.LLL_alg(basis, false);
            result += "Basis after LLL-algorithm:" + "\n"; ;
            result += "b[0] - lll-basis, b[1] - Gram_Schmidt's orthogonal vectors" + "\n";
            result += "Gram_Schmidt's orthogonal vectors:" + "\n";
            foreach (var v in b[1])
                result += v.ToString() + "\n";
            result += "lll-basis:" + "\n";
            foreach (var v in b[0])
                result += v.ToString() + "\n";
            Point_n x = point;
            result += "Point:" + x.ToString() + "\n";
            double[] r = new double[point.Count];
            double m_z = 0;
            List<double> m = new List<double>();
            List<Vector_n> res = new List<Vector_n>();
            for (int i = point.Count - 1; i >= 0; i--)
            {
                r[i] = b[1][i] * x / (b[1][i] * b[1][i]);
                result += "r[" + i.ToString() + "] = b[1][i] * x / (b[1]*[i]^2) =" + (b[0][i] * x / (b[0][i] * b[0][i])).ToString() + "\n";
                m_z = Closest_integer(r[i]);
                result += "m[" + i.ToString() + "] = Closest_integer(r[" + i.ToString() + "]) = " + Closest_integer(r[i]).ToString() + "\n";
                m.Add(m_z);
                if (i > 0)
                {
                    result += "i > 0" + "\n";
                    x = x - m_z * b[0][i] - (r[i] - m_z) * b[1][i];
                }
                result += "Point = point - m[" + i.ToString() + "] * b[0][" + i.ToString() + "] - (r[" + i.ToString() + "] - m[" + i.ToString() + "]) * b[1][" + i.ToString() + "]" + "\n";
            }
            m.Reverse();
            string str = "";
            foreach (var v in m)
                str += v.ToString() + ",";
            str = str.Remove(str.Length - 1);
            result += "m = {" + str + "}" + "\n";
            result += "Result point = b * m" + "\n";
            result += (b[1] * new Vector_n(m)).ToString() + "\n";
            return result;
        }


        private void Swap(Vector_n v1, Vector_n v2, ref List<Vector_n> vectors)
        {
            Vector_n v = v1;
            int ind1 = vectors.IndexOf(v1);
            int ind2 = vectors.IndexOf(v2);
            vectors[ind1] = v2;
            vectors[ind2] = v;
        }
        private List<Vector_n> Swap(Vector_n v1, Vector_n v2, List<Vector_n> vectors)
        {
            Vector_n v = v1;
            int ind1 = vectors.IndexOf(v1);
            int ind2 = vectors.IndexOf(v2);
            vectors[ind1] = v2;
            vectors[ind2] = v;
            return vectors;
        }
        public Vector_n Proj_z(Vector_n u, Vector_n v)
        {
            return Math.Round(v * u / (u * u)) * u;
        }
        public Dictionary<double, Vector_n> Proj_r(Vector_n u, Vector_n v)
        {
            return new Dictionary<double, Vector_n>() { { (v * u) / (u * u), ((v * u) / (u * u)) * u } };
        }
        public double Find_μ(Vector_n u, Vector_n v)
        {
            return (v * u) / (u * u);
        }
        public double Length_squared(Vector_n v)
        {
            return Math.Pow(v.Length, 2);
        }
        public double Closest_integer(double n)
        {
            if (n < 0)
            {
                if (Math.Abs(n) % 1 >= 0.5)
                    return (int)n - 1;
                else return (int)n;
            }
            else if (Math.Abs(n) % 1 >= 0.5)
                 return (int)n + 1;
                else return (int)n;
        }
    }
}
