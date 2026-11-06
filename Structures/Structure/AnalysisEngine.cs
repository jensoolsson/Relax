using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structures
{
    public class AnalysisEngine
    {
        protected Structure _structure;             //Structure
        protected double _convergeForce_DR;         //Force as convergence limit    
        protected double _convergeForce_Sizing;
        double _obf;                                //Out of balace force
        double _obm;
        bool _isConverged_DR;                       //For normal DR analysis..
        bool _isConverged_Sizing;
        bool _updateLoad;


        public void Initialise()
        {
            _convergeForce_DR = 0;
            _obf = 0;
            _obm = 0;

            _isConverged_DR = false;
            _isConverged_Sizing = false;
            _updateLoad = false;
        }

        public AnalysisEngine(Structure structure) 
        {
            _structure = structure;
        }

        public double OutOfBalanceForce 
        {
            get { return _obf; }
            set { _obf = value; }
        }

        public double OutOfBalanceMoment
        {
            get { return _obm; }
            set { _obm = value; }
        }

        public double ConvergenceForce
        {
            get { return _convergeForce_DR; }
            set { _convergeForce_DR = value; }
        }

        public Structure Structure
        {
            get { return _structure; }
            set { _structure = value; }
        }

        public bool IsConverged_DR 
        {
            get { return _isConverged_DR; }
            set { _isConverged_DR = value; }
        }

        public bool IsConverged_Sizing
        {
            get { return _isConverged_Sizing; }
            set { _isConverged_Sizing = value; }
        }

        public void Preprocess(Parameters p) 
        {
            CalcConvergenceForce(p);
        }

        /** Dynamic relaxation iteration 
        *   
        */
        public void RunDR(Parameters p) 
        {
            if (_updateLoad)
            {
                _structure.CalcLoadFromMass(p);
                _structure.CalcLoadFromSurface(p);
                _structure.ClearNodeLoad();
                _structure.ApplyLoad(p);
                _structure.CalcMaxMinLoadPerNode();
                _updateLoad = false;
            }

            //Set force = 0, and moment = 0 on the nodes
            _structure.ResetForcesMoments();

            //Calculate new ficious masses used for faster convergence in the DR
            _structure.CalcFictiousMass(p);
            
            //Calculate force and moments in the beams.
            if(!p.ModeControl)
                _structure.CalcStrain(p);
            else
                _structure.CalcForceAndMoments(p);

            //Apply force and or moments to nodes.
            _structure.ApplyStrainToNodes();

            //Translate and rotate the nodes based on out of balance force and moment.
            _structure.TransformNodes(p);

            //Calculate utilization, deflected shape and color for elements
            _structure.PostProcess(p);

            IsConverged();
        }

        

        /** Load has changed from the UI
        *   
        */
        public void UpdateLoad(Parameters p)
        {
            _updateLoad = true;
        }


        /** Size optimisation iteration 
        *   
        */
        public bool RunSizing(Parameters p)
        {
            //Size beams to withstand current utilizations
            _structure.Sizing(p);

            //Update size depenend properties
            _structure.ClearNodeLoad();
            _structure.CalcMass(p);
            _structure.CalcLoadFromMass(p);
            _structure.ApplyLoad(p);

            _structure.CalcFictiousMass(p);
            _structure.CalcTotalStructuralWeight();
            _structure.CalcCrossSectionProperties();
 
            return false;
        }

        /** Matrix solver iteration 
        *   
        */
        public bool RunMatrix() 
        {
            return true;
        }

        public void IsConverged()
        {
            _isConverged_DR = false;
            _isConverged_Sizing = false;

            _obf = 0;

            foreach (Node3Dof n in _structure.GetNode3dofs())
                if (n.Constraint == eConstraint.Free)
                    _obf += Math.Sqrt(n.Force[0] * n.Force[0] + n.Force[1] * n.Force[1] + n.Force[2] * n.Force[2]);

            if (_obf < _convergeForce_Sizing)
                _isConverged_Sizing = true;

            if (_obf < _convergeForce_DR)
                _isConverged_DR = true;
        }

        public void CalcOutOfBalanceMoment() 
        {
            _obm = 0;

            foreach (Node6Dof n in _structure.GetNode6dofs()) 
            {
                _obm += Math.Abs(n.Moment[0]);
                _obm += Math.Abs(n.Moment[1]);
                _obm += Math.Abs(n.Moment[2]);
            }
        }

        public void CalcConvergenceForce(Parameters p)
        {
            double totForce = 0;

            foreach (Element e in _structure.GetElements())
            {
                e.CalcConvergForce();
                totForce += e.ConvergForce;
            }
            _convergeForce_DR = 0.00005 * totForce;
            _convergeForce_Sizing = 0.05 * totForce;
        }

        public void RunMatrixSolver() 
        {
        
        }









    }
}
