
using System;

namespace Api.Models
{  
//This class will contain all the user information.
public class PatientExercise
{       
        public string patientid { get; set; }
       
        public string exerciseid { get; set; }
        public DateTime dateassigned { get; set; }
        public int completed { get; set; }
        public DateTime datecompleted { get; set; }
        public string score { get; set; }
        public string stutteramount { get; set; }
        public string timetaken { get; set; }
        public string resetamount { get; set; }


}

}
