using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;

namespace Flee.InternalTypes
{
    [Obsolete("Manages branch information and allows us to determine if we should emit a short or long branch")]
    internal class BranchManager
    {
        private IList<BranchInfo> MyBranchInfos;

        private IDictionary<object, Label> MyKeyLabelMap;
        public BranchManager()
        {
            MyBranchInfos = new List<BranchInfo>();
            MyKeyLabelMap = new Dictionary<object, Label>();
        }

        /// <summary>
        /// Determine whether to use short or long branches
        /// </summary>
        /// <remarks></remarks>
        public void ComputeBranches()
        {
            List<BranchInfo> betweenBranches = new List<BranchInfo>();

            foreach (BranchInfo bi in MyBranchInfos)
            {
                betweenBranches.Clear();

                // Find any branches between the start and end locations of this branch
                this.FindBetweenBranches(bi, betweenBranches);

                // Count the number of long branches in the above set
                int longBranchesBetween = this.CountLongBranches(betweenBranches);

                // Adjust the branch as necessary
                bi.AdjustForLongBranchesBetween(longBranchesBetween);
            }

            int longBranchCount = 0;

            // Adjust the start location of each branch
            foreach (BranchInfo bi in MyBranchInfos)
            {
                // Save the short/long branch type
                bi.BakeIsLongBranch();

                // Adjust the start location as necessary
                bi.AdjustForLongBranches(longBranchCount);

                // Keep a tally of the number of long branches
                longBranchCount += Convert.ToInt32(bi.IsLongBranch);
            }
        }

        /// <summary>
        /// Count the number of long branches in a set
        /// </summary>
        /// <param name="dest"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private int CountLongBranches(ICollection<BranchInfo> dest)
        {
            int count = 0;

            foreach (BranchInfo bi in dest)
            {
                count += Convert.ToInt32(bi.ComputeIsLongBranch());
            }

            return count;
        }

        /// <summary>
        /// Find all the branches between the start and end locations of a target branch
        /// </summary>
        /// <param name="target"></param>
        /// <param name="dest"></param>
        /// <remarks></remarks>
        private void FindBetweenBranches(BranchInfo target, ICollection<BranchInfo> dest)
        {
            foreach (BranchInfo bi in MyBranchInfos)
            {
                if (bi.IsBetween(target) == true)
                {
                    dest.Add(bi);
                }
            }
        }

        /// <summary>
        /// Determine if a branch from a point to a label will be long
        /// </summary>
        /// <param name="ilg"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsLongBranch(FleeILGenerator ilg, Label target)
        {
            ILLocation startLoc = new ILLocation(ilg.Length);
            BranchInfo bi = new BranchInfo(startLoc, target);

            int index = MyBranchInfos.IndexOf(bi);
            bi = MyBranchInfos[index];

            return bi.IsLongBranch;
        }

        /// <summary>
        /// Add a branch from a location to a target label
        /// </summary>
        /// <param name="ilg"></param>
        /// <param name="target"></param>
        /// <remarks></remarks>
        public void AddBranch(FleeILGenerator ilg, Label target)
        {
            ILLocation startLoc = new ILLocation(ilg.Length);

            BranchInfo bi = new BranchInfo(startLoc, target);
            MyBranchInfos.Add(bi);
        }

        /// <summary>
        /// Get a label by a key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public Label FindLabel(object key)
        {
            return MyKeyLabelMap[key];
        }

        /// <summary>
        /// Get a label by a key.  Create the label if it is not present.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ilg"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public Label GetLabel(object key, FleeILGenerator ilg)
        {
            Label lbl;
            if (MyKeyLabelMap.TryGetValue(key, out lbl) == false)
            {
                lbl = ilg.DefineLabel();
                MyKeyLabelMap.Add(key, lbl);
            }
            return lbl;
        }

        /// <summary>
        /// Determines if we have a label for a key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool HasLabel(object key)
        {
            return MyKeyLabelMap.ContainsKey(key);
        }

        /// <summary>
        /// Set the position for a label
        /// </summary>
        /// <param name="ilg"></param>
        /// <param name="target"></param>
        /// <remarks></remarks>
        public void MarkLabel(FleeILGenerator ilg, Label target)
        {
            int pos = ilg.Length;

            foreach (BranchInfo bi in MyBranchInfos)
            {
                bi.Mark(target, pos);
            }
        }

        public override string ToString()
        {
            string[] arr = new string[MyBranchInfos.Count];

            for (int i = 0; i <= MyBranchInfos.Count - 1; i++)
            {
                arr[i] = MyBranchInfos[i].ToString();
            }

            return string.Join(System.Environment.NewLine, arr);
        }
    }

    [Obsolete("Represents a location in an IL stream")]
    internal class ILLocation : IEquatable<ILLocation>, IComparable<ILLocation>
    {
        private int _myPosition;

        /// <summary>
        /// ' Long branch is 5 bytes; short branch is 2; so we adjust by the difference
        /// </summary>
        private const int LongBranchAdjust = 3;

        /// <summary>
        /// Length of the Br_s opcode
        /// </summary>
        private const int BrSLength = 2;

        public ILLocation()
        {
        }

        public ILLocation(int position)
        {
            _myPosition = position;
        }

        public void SetPosition(int position)
        {
            _myPosition = position;
        }

        /// <summary>
        /// Adjust our position by a certain amount of long branches
        /// </summary>
        /// <param name="longBranchCount"></param>
        /// <remarks></remarks>
        public void AdjustForLongBranch(int longBranchCount)
        {
            _myPosition += longBranchCount * LongBranchAdjust;
        }

        /// <summary>
        /// Determine if this branch is long
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsLongBranch(ILLocation target)
        {
            // The branch offset is relative to the instruction *after* the branch so we add 2 (length of a br_s) to our position
            return Utility.IsLongBranch(_myPosition + BrSLength, target._myPosition);
        }

        public bool Equals1(ILLocation other)
        {
            return _myPosition == other._myPosition;
        }
        bool System.IEquatable<ILLocation>.Equals(ILLocation other)
        {
            return Equals1(other);
        }

        public override string ToString()
        {
            return _myPosition.ToString("x");
        }

        public int CompareTo(ILLocation other)
        {
            return _myPosition.CompareTo(other._myPosition);
        }
    }

    [Obsolete("Represents a branch from a start location to an end location")]
    internal class BranchInfo : IEquatable<BranchInfo>
    {
        private readonly ILLocation _myStart;
        private readonly ILLocation _myEnd;
        private Label _myLabel;
        private bool _myIsLongBranch;
        public BranchInfo(ILLocation startLocation, Label endLabel)
        {
            _myStart = startLocation;
            _myLabel = endLabel;
            _myEnd = new ILLocation();
        }

        public void AdjustForLongBranches(int longBranchCount)
        {
            _myStart.AdjustForLongBranch(longBranchCount);
        }

        public void BakeIsLongBranch()
        {
            _myIsLongBranch = this.ComputeIsLongBranch();
        }

        public void AdjustForLongBranchesBetween(int betweenLongBranchCount)
        {
            _myEnd.AdjustForLongBranch(betweenLongBranchCount);
        }

        public bool IsBetween(BranchInfo other)
        {
            return _myStart.CompareTo(other._myStart) > 0 && _myStart.CompareTo(other._myEnd) < 0;
        }

        public bool ComputeIsLongBranch()
        {
            return _myStart.IsLongBranch(_myEnd);
        }

        public void Mark(Label target, int position)
        {
            if (_myLabel.Equals(target) == true)
            {
                _myEnd.SetPosition(position);
            }
        }

        public bool Equals1(BranchInfo other)
        {
            return _myStart.Equals1(other._myStart) && _myLabel.Equals(other._myLabel);
        }
        bool System.IEquatable<BranchInfo>.Equals(BranchInfo other)
        {
            return Equals1(other);
        }

        public override string ToString()
        {
            return $"{_myStart} -> {_myEnd} (L={_myStart.IsLongBranch(_myEnd)})";
        }

        public bool IsLongBranch => _myIsLongBranch;
    }
}
