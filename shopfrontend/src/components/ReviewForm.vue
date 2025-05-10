<template>
  <!-- Toasts Stack -->
  <Toast :toasts="toasts" @close="removeToast" />

  <!-- Container for the review form -->
  <div class="relative justify-center content-center p-6 bg-[#1a1a1a] text-[#C8B280] rounded-lg shadow-xl w-[90%] sm:w-[500px] border border-[#333]">
    <!-- Title -->
    <h2 class="text-xl font-semibold mb-4 text-[#F5DEB3]">
      Write a Review for {{ product?.title || 'Book' }}
    </h2>

    <!-- Inline Rating Stars -->
    <div class="flex items-center space-x-2 mb-4">
      <font-awesome-icon
          v-for="star in 5"
          :key="star"
          :icon="displayedRating >= star ? ['fas','star'] : ['far','star']"
          class="w-6 h-6 cursor-pointer text-yellow-500"
          @mouseover="handleStarHover(star)"
          @mouseleave="handleStarLeave"
          @click="handleStarClick(star)"
      />
    </div>

    <!-- Review Text Area -->
    <textarea
        v-model="review"
        placeholder="Write your review here..."
        class="w-full p-3 rounded-md text-sm bg-[#2a2a2a] text-[#C8B280] border border-[#444] focus:outline-none focus:ring-2 focus:ring-[#C8B280]"
    ></textarea>

    <!-- Action Buttons -->
    <div class="mt-4 flex items-center space-x-2">
      <button
          class="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-md transition"
          @click="submitReview"
      >
        Submit Review
      </button>

      <button
          class="bg-[#444] hover:bg-[#666] text-white px-4 py-2 rounded-md transition"
          @click="closeForm"
      >
        Close
      </button>
    </div>

    <!-- Close X -->
    <button
        @click="closeForm"
        class="absolute top-2 right-2 text-[#888] hover:text-red-500 p-2"
    >
      <svg xmlns="http://www.w3.org/2000/svg" class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
      </svg>
    </button>
  </div>
</template>

<script setup>
import { ref, defineProps, defineEmits, computed, reactive } from 'vue';
import Toast from '@/components/Toast.vue';

const props = defineProps({ product: Object });
const emit = defineEmits(['close']);

const toasts = reactive([]);
function removeToast(id) {
  const idx = toasts.findIndex(t => t.id === id);
  if (idx !== -1) toasts.splice(idx, 1);
}
function showToast(message, type = 'success') {
  const id = Date.now() + Math.random();
  toasts.push({ id, message, type });
  setTimeout(() => removeToast(id), 4000);
}

const rating = ref(0);
const hoveredRating = ref(0);
const review = ref('');

const displayedRating = computed(() =>
    hoveredRating.value > 0 ? hoveredRating.value : rating.value
);

function handleStarHover(star) { hoveredRating.value = star; }
function handleStarLeave() { hoveredRating.value = 0; }
function handleStarClick(star) { rating.value = star; }

async function submitReview() {
  if (!props.product) return;
  const token = localStorage.getItem('authToken');
  try {
    const res = await fetch(
        `${process.env.VUE_APP_API_URL}/api/review`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${token}`,
          },
          body: JSON.stringify({
            bookId: props.product.bookId || props.product.id,
            rating: rating.value,
            comment: review.value,
          })
        }
    );

    const data = await res.json();
    if (!res.ok) {
      showToast(data.message || 'Failed to submit review.', 'error');
      return;
    }

    showToast(data.message || 'Review submitted successfully!', 'success');
    rating.value = 0;
    hoveredRating.value = 0;
    review.value = '';

    // Delay closing so toast appears
    setTimeout(() => emit('close'), 1500);
  } catch (err) {
    console.error(err);
    showToast('Network error submitting review.', 'error');
  }
}

function closeForm() {
  emit('close');
}
</script>

<style scoped>
</style>