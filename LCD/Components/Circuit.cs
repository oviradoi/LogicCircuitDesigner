using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LCD.Components.Gates;
using LCD.UndoRedo;

namespace LCD.Components
{
    [Serializable]
    public class Circuit:IUndoRedo
    {
        public List<Gate> Gates { get; set; }
        public List<Wire> Wires { get; set; }

        [NonSerialized]
        private UndoRedoObject<List<Gate>> undoRedoGates = new UndoRedoObject<List<Gate>>();
        [NonSerialized]
        private UndoRedoObject<List<Wire>> undoRedoWires = new UndoRedoObject<List<Wire>>();
        
        public Circuit()
        {
            Gates = new List<Gate>();
            Wires = new List<Wire>();
        }

        private void InitializeUndoRedo()
        {
            undoRedoGates = new UndoRedoObject<List<Gate>>();
            undoRedoWires = new UndoRedoObject<List<Wire>>();
        }

        public void Simulate()
        {
            foreach (Gate g in Gates)
            {
                g.Simulate();
            }
        }

        public int CountButtons()
        {
            int nr = 0;
            foreach (Gate g in Gates)
                if (g is Btn)
                    nr++;
            return nr;
        }

        public int CountLeds()
        {
            int nr = 0;
            foreach (Gate g in Gates)
                if (g is Led)
                    nr++;
            return nr;
        }

        public Btn GetButton(int idx)
        {
            int nr = 0;
            foreach (Gate g in Gates)
                if (g is Btn)
                {
                    if (nr == idx)
                        return (Btn)g;
                    nr++;
                }
            return null;
        }

        public Led GetLed(int idx)
        {
            int nr = 0;
            foreach (Gate g in Gates)
                if (g is Led)
                {
                    if (nr == idx)
                        return (Led)g;
                    nr++;
                }
            return null;
        }

        #region IUndoRedo Members

        public void Undo()
        {
            List<Gate> undoGateList = undoRedoGates.Undo();

            if (undoGateList != null)
            {
                this.Gates = undoGateList;
            }

            List<Wire> undoWireList = undoRedoWires.Undo();

            if (undoWireList != null)
            {
                this.Wires = undoWireList;
            }
        }

        public void Redo()
        {
            List<Gate> redoGateList = undoRedoGates.Redo();

            if (redoGateList != null)
            {
                this.Gates = redoGateList;
            }

            List<Wire> redoWireList = undoRedoWires.Redo();

            if (redoWireList != null)
            {
                this.Wires = redoWireList;
            }
        }

        public void SaveState()
        {
            List<Gate> newGateList = new List<Gate>(Gates);
            List<Wire> newWireList = new List<Wire>(Wires);

            if (undoRedoGates == null &&
                undoRedoWires == null)
            {
                InitializeUndoRedo();
            }

            undoRedoGates.SaveState(newGateList);
            undoRedoWires.SaveState(newWireList);
        }

        #endregion
    }
}