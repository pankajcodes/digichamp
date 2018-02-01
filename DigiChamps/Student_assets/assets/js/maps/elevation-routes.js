 var map;
    var mousemarker = null;

    // Load the Visualization API and the piechart package.
    google.load("visualization", "1", {packages: ["columnchart"]});

    $(document).ready(function(){
      map = new GMaps({
        el: '#ele_map',
        lat: -12.043333,
        lng: -77.028333,
        zoom: 12
      });

      //load google visualization chart
      chart = new google.visualization.ColumnChart(document.getElementById('chart_div'));

      var polygone = map.drawRoute({
        origin: [-12.044012922866312, -77.02470665341184],
        destination: [-12.090814532191756, -77.02271108990476],
        travelMode: 'walking',
        strokeColor: '#131540',
        strokeOpacity: 0.6,
        strokeWeight: 6,
        callback : function(polygones){

          //elevation for the path
          map.getElevations({
            locations : polygones.overview_path,
            path: true, 
            callback : function (result, status){
              if (status == google.maps.ElevationStatus.OK) {

                var elevations = result;

                //set the google visualization
                var data = new google.visualization.DataTable();
                data.addColumn('string', 'Sample');
                data.addColumn('number', 'Elevation');
                for (var i = 0; i < result.length; i++) {
                  data.addRow(['', elevations[i].elevation]);
                }

                //add to the dom 
                document.getElementById('chart_div').style.display = 'block';
                chart.draw(data, {
                  width: 340,
                  height: 200,
                  legend: 'none',
                  titleY: 'Elevation (m)',
                  focusBorderColor: '#00ff00'
                });

                //add mouseover
                google.visualization.events.addListener(chart, 'onmouseover', function(e) {
                  if (mousemarker == null) {
                    mousemarker = map.addMarker({
                      lat: elevations[e.row].location.lat(),
                      lng: elevations[e.row].location.lng()
                    });
                  } else {
                    mousemarker.setPosition(elevations[e.row].location);
                  }
                });
              }
            }
          });
        }
      });
    });
