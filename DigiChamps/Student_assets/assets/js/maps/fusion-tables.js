var map, infoWindow;
    $(document).ready(function(){
      infoWindow = new google.maps.InfoWindow({});
      map = new GMaps({
        el: '#fusion_tables',
        zoom: 11,
        lat: 41.850033,
        lng: -87.6500523
      });
      map.loadFromFusionTables({
        query: {
          select: '\'Geocodable address\'',
          from: '1mZ53Z70NsChnBMm-qEYmSDOvLXgrreLTkQUvvg'
        },
        suppressInfoWindows: true,
        events: {
          click: function(point){
            infoWindow.setContent('You clicked here!');
            infoWindow.setPosition(point.latLng);
            infoWindow.open(map.map);
          }
        }
      });
    });