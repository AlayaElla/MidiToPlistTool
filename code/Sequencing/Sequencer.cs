/*
 * Created by: Leslie Sanford
 * 
 * Contact: jabberdabber@hotmail.com
 * 
 * Last modified: 05/08/2004
 */

using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;

namespace Multimedia.Midi
{
	/// <summary>
	/// Summary description for Sequencer
	/// </summary>
	public class Sequencer : Component
	{
        // The sequence to play.
        private Sequence sequence;

        // Midi sender for sending Midi messages.
        //private IMidiSender midiSender;

        private int position = 0;

        // Current Midi event index into the sequence.
        private int currIndex = 0;

        // Current tick count.
        private int currTicks = 0;

        // Running status value.
        private int runningStatus = 0;

        // Merged track from the sequence for playback.
        private Track mergedTrack = new Track();  
      
        public int Position
        {
            get
            {
                return position;
            }
            set
            {
                // Enforce preconditions.
                if(value < 0)
                    throw new ArgumentOutOfRangeException("Position", value,
                        "Negative values for sequencer position not allowed.");

                // Initialize position.
                position = value;

                // If the sequencer has a sequence.
                if(Sequence != null)
                { 
                    // Tick counter.
                    int p = 0;                        

                    currIndex = currTicks = 0;

                    // Get merged track from sequence.
                    mergedTrack = sequence.GetMergedTrack();
                        
                    // Find the Midi event at the new position in merged track.
                    while(currIndex < mergedTrack.Count && p < position)
                    {
                        // Accumulate ticks from Midi events.
                        p += mergedTrack[currIndex].Ticks;

                        // If the new position has not been reached yet.
                        if(p < position)
                        {
                            // Move to next event.
                            currIndex++;
                        }
                    }

                    // If the end of the merged track has not been reached.
                    if(currIndex < mergedTrack.Count)
                    {
                        //
                        // Calculate the current tick count based on the 
                        // new position.
                        //

                        p -= position;
                        currTicks = mergedTrack[currIndex].Ticks - p;
                    }
                }
            }
        }

        public Sequence Sequence
        {
            get
            {
                return sequence;
            }
            set
            {
                // Enforce preconditions.
                if(value != null && value.IsSmpte())
                    throw new ArgumentException(
                        "Smpte sequences are not yet supported");


                // Initialize new sequence.
                sequence = value;         
            }
        }
	}
}