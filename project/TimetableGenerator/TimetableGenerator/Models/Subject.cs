/**
 * 
 * Description: This class is the subject model, this model contains the name of the subject.
 * 
 * Author: Adam Chen
 * Date: 2025/07/31
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimetableGenerator.Models
{
    public class Subject
    {
        // Name of the subject
        public string Name { get; set; }

        // Constructor to initialize the subject with a name
        public Subject(string name)
        {
            Name = name;
        }


        public override string ToString()
        {
            // Return the name of the subject for display purposes
            return Name;
        }
    }
}
    
