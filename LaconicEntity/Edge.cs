namespace WarLight.AI.LaconicEntity {

    public class Edge {
        
        TerritoryIDType from, to;
        int weight;

        public Edge(TerritoryIDType from, TerritoryIDType to) {
            this.from = from;
            this.to = to;
            weight = 0;
        }

        public Edge(TerritoryIDType from, TerritoryIDType to, int weight) {
            this.from = from;
            this.to = to;
            this.weight = weight;
        }

    }

}