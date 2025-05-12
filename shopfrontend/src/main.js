import { createApp } from 'vue';
import App from './App.vue';
import router from './router';
import './style.css';

// Import Font Awesome
import { library } from '@fortawesome/fontawesome-svg-core';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';


// Import specific Font Awesome icons
import {
    faShoppingCart,
    faStar,
    faStarHalfAlt,
    faUser,       // Profile icon
    faSignOutAlt, // Logout icon
    faBell,        // Notification Bell icon
    faHeart
} from '@fortawesome/free-solid-svg-icons';
import { faStar as faStarOutline } from '@fortawesome/free-regular-svg-icons';

// Add icons to the library
library.add(faShoppingCart, faStar, faStarHalfAlt, faStarOutline, faUser, faSignOutAlt, faBell,faHeart);

// Create Vue app and register Font Awesome globally
createApp(App)
    .component('font-awesome-icon', FontAwesomeIcon) // Register Font Awesome component globally
    .use(router)
    .mount('#app');
