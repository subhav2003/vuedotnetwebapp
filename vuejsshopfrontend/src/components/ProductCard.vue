<template>
  <div
      v-if="book"
      class="relative max-w-xs w-full mx-auto bg-[#1a1a1a] rounded-lg shadow-lg hover:shadow-xl transform hover:scale-105 transition duration-300 ease-in-out text-[#C8B280]"
  >
    <!-- Banners -->
    <div class="absolute top-2 left-2 flex flex-col gap-2 z-10">
      <div
          v-if="isForSale"
          class="bg-red-600 text-white text-xs font-bold px-3 py-1 rounded-full shadow-md animate-pulse"
      >
        SALE - {{ Math.round(book.discountPercentage) }}% OFF
      </div>
      <div
          v-if="book.stock < 1"
          class="bg-gray-800 text-white text-xs font-bold px-3 py-1 rounded-full shadow"
      >
        OUT OF STOCK
      </div>
      <div
          v-if="book.isExclusiveEdition"
          class="bg-yellow-400 text-black text-xs font-bold px-3 py-1 rounded-full shadow"
      >
        EXCLUSIVE
      </div>
    </div>

    <!-- Image Carousel -->
    <div class="relative w-full aspect-[4/3] bg-[#2a2a2a] flex items-center justify-center overflow-hidden rounded-t-lg">
      <div
          class="flex transition-transform duration-500 ease-in-out h-full w-full"
          :style="{ transform: `translateX(-${currentImageIndex * 100}%)` }"
      >
        <div
            v-for="(image, index) in imageList"
            :key="index"
            class="flex-shrink-0 w-full h-full flex items-center justify-center"
        >
          <img
              :src="getFullImageUrl(image)"
              alt="Book Image"
              class="max-w-full max-h-full object-contain"
          />
        </div>
      </div>

      <button
          v-if="imageList.length > 1"
          @click.stop="prevImage"
          class="absolute left-2 top-1/2 transform -translate-y-1/2 bg-[#444] text-white p-2 rounded-full hover:bg-[#666]"
      >
        &lt;
      </button>
      <button
          v-if="imageList.length > 1"
          @click.stop="nextImage"
          class="absolute right-2 top-1/2 transform -translate-y-1/2 bg-[#444] text-white p-2 rounded-full hover:bg-[#666]"
      >
        &gt;
      </button>
    </div>

    <!-- Book Info -->
    <div class="p-4">
      <!-- Title -->
      <h2
          @click="emitDetail"
          class="font-bold text-lg cursor-pointer hover:text-white truncate"
          :title="book.title"
      >
        {{ book.title }}
      </h2>

      <!-- Author -->
      <p class="text-sm text-[#d1c2a0] mb-1 truncate" :title="book.author">
        by: {{ book.author }}
      </p>

      <!-- Rating (static for now) -->
      <div class="flex items-center space-x-1 mb-3">
        <font-awesome-icon
            v-for="starIndex in 5"
            :key="starIndex"
            :icon="['far', 'star']"
            class="w-4 h-4 text-yellow-400"
        />
      </div>

      <!-- Price + Slot for buttons -->
      <div class="flex justify-between items-center">
        <span class="font-semibold text-xl text-[#C8B280]">
          ${{ Number(book.price || 0).toFixed(2) }}
        </span>
        <slot name="action-button" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, defineProps, defineEmits, onMounted, computed } from 'vue';

const props = defineProps({
  book: { type: Object, required: true }
});
const emit = defineEmits(['showBookDetail']);

const currentImageIndex = ref(0);
const backendUrl = process.env.VUE_APP_API_URL;
const placeholder = 'https://via.placeholder.com/400x300';

const imageList = computed(() =>
    Array.isArray(props.book?.images) ? props.book.images : []
);

const getFullImageUrl = (imagePath) => {
  if (!imagePath) return placeholder;
  return imagePath.startsWith('http') ? imagePath : `${backendUrl}${imagePath}`;
};

const prevImage = () => {
  if (imageList.value.length > 0) {
    currentImageIndex.value =
        currentImageIndex.value === 0
            ? imageList.value.length - 1
            : currentImageIndex.value - 1;
  }
};

const nextImage = () => {
  if (imageList.value.length > 0) {
    currentImageIndex.value = (currentImageIndex.value + 1) % imageList.value.length;
  }
};

onMounted(() => {
  if (imageList.value.length > 1) {
    setInterval(nextImage, 5000);
  }
});

const emitDetail = () => emit('showBookDetail', props.book);

const isForSale = computed(() => {
  const now = new Date();
  return (
      props.book.isOnSale &&
      new Date(props.book.discountStart) <= now &&
      new Date(props.book.discountEnd) >= now
  );
});
</script>

<style scoped>
img {
  object-fit: contain;
}
</style>