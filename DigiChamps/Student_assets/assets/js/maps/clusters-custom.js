 var map;
    $(document).ready(function(){
      map = new GMaps({
        div: '#cluster',
        lat: -12.043333,
        lng: -77.028333,
        markerClusterer: function(map) {
          return new MarkerClusterer(map);
        }
      });

      var lat_span = -12.035988012939503 - -12.050677786181573;
      var lng_span = -77.01528673535154 - -77.04137926464841;

      for(var i = 0; i < 100; i++) {
        var latitude = Math.random()*(lat_span) + -12.050677786181573;
        var longitude = Math.random()*(lng_span) + -77.04137926464841;

        map.addMarker({
          lat: latitude,
          lng: longitude,
          title: 'Marker #' + i
        });
      }
    });