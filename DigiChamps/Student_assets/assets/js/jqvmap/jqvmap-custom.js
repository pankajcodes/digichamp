jQuery(document).ready(function() {
		jQuery('#vmap_usa').vectorMap({
		    map: 'usa_en',
		    backgroundColor: '#fff',
    borderColor: '#fff',
    borderOpacity: 0,
    borderWidth: 3,
    color: '#e86219',
    enableZoom: true,
    hoverColor: '#56bfeb',
    hoverOpacity: null,
    normalizeFunction: 'linear',
    scaleColors: ['#e15b77', '#e15b77'],
    selectedColor: '#56bfeb',
    selectedRegion: 'TX',
    showTooltip: true,
    
		});

			jQuery('#vmap_europe').vectorMap({
		    map: 'europe_en',
		    backgroundColor: '#fff',
    borderColor: '#fff',
    borderOpacity: 0,
    borderWidth: 3,
    color: '#ffd040',
    enableZoom: true,
    hoverColor: '#56bfeb',
    hoverOpacity: null,
    normalizeFunction: 'linear',
    scaleColors: ['#e15b77', '#e15b77'],
    selectedColor: '#56bfeb',
    selectedRegion: null,
    showTooltip: true,
    
		});

				jQuery('#vmap_germany').vectorMap({
		    map: 'germany_en',
		    backgroundColor: '#fff',
    borderColor: '#fff',
    borderOpacity: 0,
    borderWidth: 4,
    color: '#ff4db6',
    enableZoom: true,
    hoverColor: '#56bfeb',
    hoverOpacity: null,
    normalizeFunction: 'linear',
    scaleColors: ['#e15b77', '#e15b77'],
    selectedColor: '#56bfeb',
    selectedRegion: null,
    showTooltip: true,
    
		});

		jQuery('#vmap_asia').vectorMap({
		    map: 'asia_en',
		    backgroundColor: '#fff',
    borderColor: '#fff',
    borderOpacity: 0,
    borderWidth: 4,
    color: '#124ee7',
    enableZoom: true,
    hoverColor: '#56bfeb',
    hoverOpacity: null,
    normalizeFunction: 'linear',
    scaleColors: ['#e15b77', '#e15b77'],
    selectedColor: '#56bfeb',
    selectedRegion: null,
    showTooltip: true,
    
		});


		jQuery('#vmap_australia').vectorMap({
		    map: 'australia_en',
		    backgroundColor: '#fff',
    borderColor: '#fff',
    borderOpacity: 0,
    borderWidth: 4,
    color: '#66cc33',
    enableZoom: true,
    hoverColor: '#56bfeb',
    hoverOpacity: null,
    normalizeFunction: 'linear',
    scaleColors: ['#e15b77', '#e15b77'],
    selectedColor: '#56bfeb',
    selectedRegion: null,
    showTooltip: true,
    
		});

		jQuery('#vmap_africa').vectorMap({
		    map: 'africa_en',
		    backgroundColor: '#fff',
    borderColor: '#fff',
    borderOpacity: 0,
    borderWidth: 4,
    color: '#e72012',
    enableZoom: true,
    hoverColor: '#56bfeb',
    hoverOpacity: null,
    normalizeFunction: 'linear',
    scaleColors: ['#e15b77', '#e15b77'],
    selectedColor: '#56bfeb',
    selectedRegion: null,
    showTooltip: true,
    
		});

		jQuery('#vmap_world').vectorMap({
		    map: 'world_en',
		    backgroundColor: '#fff',
    borderColor: '#fff',
    borderOpacity: 0,
    borderWidth: 1,
    color: '#45aff2',
    enableZoom: true,
    hoverColor: '#56bfeb',
    hoverOpacity: null,
    normalizeFunction: 'linear',
    scaleColors: ['#e15b77', '#e15b77'],
    selectedColor: '#56bfeb',
    selectedRegion: null,
    showTooltip: true,
    
		});
	});