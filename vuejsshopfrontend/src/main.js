// main.js
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
    faBell        // Notification Bell icon
} from '@fortawesome/free-solid-svg-icons';
import { faStar as faStarOutline } from '@fortawesome/free-regular-svg-icons';

// ** ADD THESE LINES: **
import { faHeart } from '@fortawesome/free-solid-svg-icons';

// Add icons to the library
library.add(
    faShoppingCart,
    faStar,
    faStarHalfAlt,
    faStarOutline,
    faUser,
    faSignOutAlt,
    faBell,
    // ** and add heart here **
    faHeart
);

// Create Vue app and register Font Awesome globally
createApp(App)
    .component('font-awesome-icon', FontAwesomeIcon)
    .use(router)
    .mount('#app');
