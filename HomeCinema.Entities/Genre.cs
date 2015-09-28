using System.Collections.Generic;

namespace HomeCinema.Entities
{
    public class Genre : IEntityBase
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}
