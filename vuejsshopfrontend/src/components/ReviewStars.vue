<template>
  <div class="flex items-center space-x-1">
    <!-- Generate clickable stars -->
    <font-awesome-icon
        v-for="(icon, index) in stars"
        :key="index"
        :icon="icon"
        class="w-6 h-6 text-yellow-500 cursor-pointer"
        @click="emitRating(index + 1)"
        @mouseover="handleHover(index)"
        @mouseleave="handleMouseOut"
    />
  </div>
</template>

<script setup>
import { ref, computed, defineProps, defineEmits } from 'vue';

const props = defineProps({
  rating: {
    type: Number,
    default: 0,
  },
});

const emit = defineEmits(['update-rating']);

// Track the currently selected rating
const selectedRating = ref(props.rating);
// Track the hovered rating for highlight effect
const hoveredRating = ref(0);

// Emit rating when a star is clicked
const emitRating = (rating) => {
  selectedRating.value = rating;
  emit('update-rating', rating);
};

// Computed stars array (full or empty) based on hovered/selected rating
const stars = computed(() => {
  const starList = [];
  // Use hovered rating if set, otherwise use selected rating
  const ratingValue = hoveredRating.value > 0 ? hoveredRating.value : selectedRating.value;

  for (let i = 0; i < 5; i++) {
    if (i < ratingValue) {
      starList.push(['fas', 'star']); // Full star
    } else {
      starList.push(['far', 'star']); // Empty star
    }
  }

  return starList;
});

// Handle star hover
const handleHover = (index) => {
  hoveredRating.value = index + 1;
};

// Reset hover rating after mouse out
const handleMouseOut = () => {
  hoveredRating.value = 0;
};
</script>

<style scoped>
.cursor-pointer:hover {
  cursor: pointer;
}
</style>
