var map;
    var route;
    $(document).ready(function(){
      $('#forward').attr('disabled', 'disabled');
      $('#back').attr('disabled', 'disabled');
      $('#get_route').click(function(e){
        e.preventDefault();
        map.getRoutes({
          origin: [map.markers[0].getPosition().lat(), map.markers[0].getPosition().lng()],
          destination: [map.markers[map.markers.length-1].getPosition().lat(), map.markers[map.markers.length-1].getPosition().lng()],
          callback: function(e){
            route = new GMaps.Route({
              map: map,
              route: e[0],
              strokeColor: '#336699',
              strokeOpacity: 0.5,
              strokeWeight: 10
            });
            $('#forward').removeAttr('disabled');
            $('#back').removeAttr('disabled');
          }
        });
        $('#forward').click(function(e){
          e.preventDefault();
          route.forward();

          if(route.step_count < route.steps_length)
            $('#steps').append('<li>'+route.steps[route.step_count].instructions+'</li>');
        });
        $('#back').click(function(e){
          e.preventDefault();
          route.back();

          if(route.step_count >= 0)
            $('#steps').find('li').last().remove();
        });
      });
      map = new GMaps({
        el: '#travel_map',
        lat: -12.043333,
        lng: -77.028333,
        zoom: 16,
        height: '500px',
        click: function(e){
          map.addMarker({
            lat: e.latLng.lat(),
            lng: e.latLng.lng()
          });
        }
      });
    });