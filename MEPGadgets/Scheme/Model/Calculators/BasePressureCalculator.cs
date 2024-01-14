using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnitsNet;
using NRPUtils.MVVMBase;


namespace MEPGadgets.Scheme.Model
{
    public abstract class BasePressureCalculator:NotifyObject,ICalculator
    {
        private VolumeFlow flowValue;
        private double intakePressure;
        private double outletPressure;
        private double pressureLoss;
        private double flowVelocity;

        public double IntakePressure 
        {
            get => intakePressure;
            set
            {
                intakePressure = value;
                OnPropertyChanged();
            } 
        }
        public double OutletPressure 
        {
            get => outletPressure;
            private set 
            {
                outletPressure = value;
                OnPropertyChanged();
            }
        }
        public double PressureLoss 
        {
            get => pressureLoss;
            private set
            {
                pressureLoss = value;
                OnPropertyChanged();
            }
        }
        public VolumeFlow FlowValue
        {
            get => flowValue;
            set
            {
                flowValue = value;
            }
        }
        public double FlowVelocity
        {
            get { return flowVelocity; }
            private set { flowVelocity = value; OnPropertyChanged(); }
        }

        public BasePressureCalculator()
        {
            PropertyChanged += IntakePressure_PropertyChanged;
        }

        private void IntakePressure_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName==nameof(IntakePressure)) 
                OutletPressure = IntakePressure+PressureLoss;
        }

        public abstract void CalculatePressureRate();

        private void FlowValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName==nameof(FlowValue))
            {
                CalculatePressureRate();
            }
        }

        public void Calculate()
        {
            throw new NotImplementedException();
        }
    }
}