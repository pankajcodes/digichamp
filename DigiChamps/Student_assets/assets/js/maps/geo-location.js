 var map_geo;
    $(document).ready(function(){
      map_geo = new GMaps({
        el: '#geo_location',
        lat: -12.043333,
        lng: -77.028333
      });
      $('#geocoding_form').submit(function(e){
        e.preventDefault();
        GMaps.geocode({
          address: $('#address').val().trim(),
          callback: function(results, status){
            if(status=='OK'){
              var latlng = results[0].geometry.location;
              map_geo.setCenter(latlng.lat(), latlng.lng());
              map_geo.addMarker({
                lat: latlng.lat(),
                lng: latlng.lng()
              });
            }
          }
        });
      });
    });