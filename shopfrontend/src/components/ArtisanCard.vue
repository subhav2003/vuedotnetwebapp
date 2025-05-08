<template>
  <div
      class="max-w-xs w-full bg-white rounded-xl shadow-lg transform transition-transform duration-300 hover:-translate-y-2 hover:shadow-2xl flex flex-col items-center p-6 space-y-4"
  >
    <!-- Profile Image -->
    <div class="w-28 h-28 rounded-full overflow-hidden ring-4 ring-brown-200">
      <img
          :src="currentImageUrl"
          alt="Profile Image"
          class="object-cover w-full h-full"
          @error="handleImageError"
      />
    </div>

    <!-- Artisan Name -->
    <h2 class="text-xl font-semibold text-gray-800 truncate text-center">
      {{ artisan.name || 'Unnamed Artisan' }}
    </h2>

    <!-- Star Rating Display -->
    <div class="flex items-center space-x-1">
      <font-awesome-icon
          v-for="(icon, idx) in stars"
          :key="idx"
          :icon="icon"
          class="w-5 h-5 text-yellow-400"
      />
    </div>

    <!-- ID -->
    <p class="text-sm text-gray-500">ID: {{ artisan.id }}</p>

    <!-- Accepts Custom Orders -->
    <p class="text-sm">
      <span class="font-medium">Accepts Custom Orders:</span>
      <span class="ml-1">{{ artisan.accept_custom_order ? 'Yes' : 'No' }}</span>
    </p>

    <!-- View Profile Button -->
    <button
        @click="viewProfile"
        class="mt-auto w-full bg-brown-500 text-white text-sm py-2 rounded-lg hover:bg-brown-600 transition-colors"
    >
      View Profile
    </button>
  </div>
</template>

<script setup>
import { defineProps, defineEmits, ref, computed } from 'vue';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';

// Register FontAwesomeIcon locally
const components = { FontAwesomeIcon };

const props = defineProps({
  artisan: {
    type: Object,
    required: true
  }
});
const emit = defineEmits(['view-profile']);

// Emit view-profile event
function viewProfile() {
  emit('view-profile', props.artisan.id);
}

// Image fallback logic
const currentImageUrl = ref(
    props.artisan.profile_image_url || getFallbackImage(props.artisan.name)
);
function handleImageError() {
  currentImageUrl.value = getFallbackImage(props.artisan.name);
}
function getFallbackImage(name) {
  const initials = encodeURIComponent(name || 'Artisan');
  return `https://ui-avatars.com/api/?name=${initials}&background=999&color=fff&size=256`;
}

// Computed star icons for display-only rating
const stars = computed(() => {
  const fullStars = Math.round(props.artisan.average_rating || 0);
  return Array.from({ length: 5 }, (_, i) => {
    return i < fullStars ? ['fas', 'star'] : ['far', 'star'];
  });
});
</script>

<style scoped>
.bg-brown-500 { background-color: #A0522D; }
.hover\:bg-brown-600:hover { background-color: #8B4513; }
.ring-brown-200 { --tw-ring-color: #d3b8ae; }
</style>