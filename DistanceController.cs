// HTTP GET Method that calculates the shorstest route between two stations
// Dijkstra's Algorithm Component is used to perform the calculation
        [HttpGet]
        [Route("api/DistanceCalc/{from}/{to}")]
        public HttpResponseMessage Get(string from, string to)
        {
            try
            {
                using (JourneyPlanningDBEntities entities = new JourneyPlanningDBEntities())
                {
                    var stationsList = entities.Stations.ToList();
                    int totaLDistance = 0;
                    int journeyTimeMin = 0;
                    double totaLDistanceKm = 0;

                    List<JourneyCalCData> stationsListData = new List<JourneyCalCData>();
                    List<stationDistance> stationDistanceList = new List<stationDistance>();
                    List<StationsList> stationsData = new List<StationsList>();
                    Graph stationsGraph = new Graph();

                    // LINQ  query fetch data between two stations
                    var dataTwoStations = (from s in entities.Stations
                                           from s1 in entities.Stations
                                           from c in entities.Connections.
                                           Where(x => x.stationIdA == s.stationId && x.stationIdB == s1.stationId)
                                           select new StationsConnections
                                           {
                                               StationNameFrom = s.stationName,
                                               StationNameTo = s1.stationName,
                                               StationDistance = (int)c.distance
                                           }).ToList();

                    // Nested Foreach loop to get the next station and distance for each station created in the trainline system 
                    foreach (var item in stationsList)
                    {
                        foreach (var val in dataTwoStations)
                        {
                            if (item.stationName == val.StationNameFrom)
                            {
                                List<Connex> connectionsList = new List<Connex>();

                                var query = dataTwoStations.Where(e => e.StationNameFrom == item.stationName);

                                foreach (var i in query)
                                {
                                    connectionsList.Add(new Connex(i.StationNameTo, i.StationDistance));
                                }
                                // list containing each staion and connections
                                stationsData.Add(new StationsList() { StationName = item.stationName, connex = connectionsList.ToArray() });
                            }
                        }
                    }
                    //LINQ query to group stations
                    var stationsQuery = stationsData.GroupBy(i => i.StationName).Select(t => t.First()).ToList();

                    foreach (var y in stationsQuery)
                    {
                        Dictionary<string, int> values = new Dictionary<string, int>();

                        foreach (var n in y.connex)
                        {
                            values.Add(n.StationTo, n.StationDis);
                        }
                        stationsGraph.add_vertex(y.StationName, values);
                    }

                    var stationsDistList = stationsGraph.shortest_path(from, to);

                    foreach (KeyValuePair<string, int> pair in stationsDistList)
                    {
                        stationDistanceList.Add(new stationDistance(pair.Key.ToString(), pair.Value));
                        totaLDistance += pair.Value;
                    }

                    stationDistanceList.Reverse();
                    totaLDistanceKm = Math.Round(totaLDistance * 1.60934, 2);
                    journeyTimeMin = totaLDistance * 2;

                    // List returns total distance in km and miles as well as an array containing the connections(name and distance)
                    stationsListData.Add(new JourneyCalCData()
                    {
                        totalDistanceMilles = totaLDistance,
                        totalDistancekm = totaLDistanceKm,
                        journeyTime = journeyTimeMin,
                        stationsDistanceList = stationDistanceList.ToArray()
                    });

                    stationsListData.ToList();
                    // return 200 ok and list
                    return Request.CreateResponse(HttpStatusCode.OK, stationsListData);
                }

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }

        }